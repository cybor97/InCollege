using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Headers;

namespace InCollege.Server
{
    internal class DataHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            var request = context.Request;
            if (request.Method == HttpMethods.Post)
                if (request.Post.Parsed.TryGetByName("token", out string tokenString))
                {
                    var validationResult = AuthorizationHandler.VerifyToken(tokenString, false);
                    if (validationResult.valid)
                    {
                        if ((DateTime.Now.Subtract(validationResult.account.LastAction ?? new DateTime()).TotalSeconds) > Account.OnlineTimeoutSeconds / 2)
                            validationResult.account.LastAction = DateTime.Now;
                        DBHolderSQL.Save(nameof(Account), validationResult.account.Columns.ToArray());
                        if (request.Post.Parsed.TryGetByName("action", out string action))
                            context.Response = Actions[action]?.Invoke(request.Post.Parsed, validationResult.account);
                        else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Эм.. что от меня требуется???", false);
                    }
                }
                else context.Response = new HttpResponse(HttpResponseCode.Forbidden, "Доступ запрещен! Нужен токен!", false);
            else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Метод недоступен!", false);

            return Task.Factory.GetCompleted();
        }

        static Dictionary<string, Func<IHttpHeaders, Account, HttpResponse>> Actions = new Dictionary<string, Func<IHttpHeaders, Account, HttpResponse>>()
        {
            { "GetRange", GetRangeProcessor },
            { "Save", SaveProcessor },
            { "Remove", RemoveProcessor }
        };

        //TODO:Improve data protection
        #region Here
        static HttpResponse GetRangeProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, false))
                if (query.TryGetByName("table", out string table))
                {
                    int skip = query.TryGetByName("skipRecords", out int skipResult) ? skipResult : 0;
                    int count = query.TryGetByName("countRecords", out int countResult) ? countResult : -1;
                    string column = query.TryGetByName("column", out string columnResult) ? columnResult : null;
                    bool fixedString = query.TryGetByName("fixedString", out int fixedStringResult) && fixedStringResult == 1;
                    bool justCount = query.TryGetByName("justCount", out int justCountResult) && justCountResult == 1;
                    bool reverse = query.TryGetByName("reverse", out int reverseResult) && reverseResult == 1;
                    var whereParams = new List<(string, object)>();

                    foreach (var current in query)
                        if (current.Key.StartsWith("where"))
                            whereParams.Add((current.Key.Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries)[0], current.Value));

                    var range = DBHolderSQL.GetRange(table, column, skip, count, fixedString, justCount, reverse, whereParams.ToArray());

                    #region Special rules for accounts
                    //We never should send passwords...
                    //...but if it's just count requested - why not?
                    if (table == nameof(Account) && !justCount)
                        foreach (DataRow current in range.Rows)
                            current[nameof(Account.Password)] = null;
                    #endregion

                    #region Special rules for messages
                    //All messages, sent us from other person have to be set "IsRead"
                    //It doesn't touches justCount queries
                    //Need to be replaced with different Processor
                    if (table == nameof(Message) && !justCount)
                        foreach (DataRow current in range.Rows)
                            if ((current[nameof(Message.IsRead)] == DBNull.Value || (long)current[nameof(Message.IsRead)] == 0) && (long)current[nameof(Message.FromID)] != account.ID)
                            {
                                current[nameof(Message.IsRead)] = 1;
                                DBHolderSQL.Save(table, current.ItemArray.Select((c, i) => (range.Columns[i].ColumnName, c)).ToArray());
                            }
                    #endregion

                    if (!justCount)
                        return new HttpResponse(HttpResponseCode.Ok, JsonConvert.SerializeObject(range, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), true);
                    else return new HttpResponse(HttpResponseCode.Ok, range.Rows[0][0].ToString(), true);
                }
                else return new HttpResponse(HttpResponseCode.BadRequest, "Ошибка! Таблица не найдена!", false);
            else return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на получение данных!", false);
        }

        static HttpResponse SaveProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
                if (query.TryGetByName("table", out string table))
                {
                    var fields = new List<(string name, object value)>();
                    foreach (var current in query)
                        if (current.Key.StartsWith("field"))
                            fields.Add((current.Key.Split(new[] { "field" }, StringSplitOptions.RemoveEmptyEntries)[0], current.Value));

                    #region Special rules for accounts. Need to be optimized.
                    if (table.Equals(nameof(Account)))
                    {
                        //Empty password field means that user don't want to change password.
                        for (int i = 0; i < fields.Count; i++)
                            if (fields[i].name.Equals("Password") && string.IsNullOrWhiteSpace((string)fields[i].value))
                                fields[i] = ("Password", account.Password);

                        //Only admin can edit foreign accounts
                        //Aand admin can do really anything with them
                        if (account.AccountType != AccountType.Admin || !account.Approved)
                        {
                            if (account.ID != (int)fields.Find(c => c.name == "ID").value)
                                return new HttpResponse(HttpResponseCode.Forbidden, "Вы не можете редактировать данные другого аккаунта!", false);

                            for (int i = 0; i < fields.Count; i++)
                                if (fields[i].name.Equals("Approved")) fields[i] = ("Approved", account.Approved);

                            //Account shouldn't be approved after AccountType switch.
                            if (query.TryGetByName("fieldAccountType", out byte accountType) &&
                                accountType != (byte)account.AccountType)
                                for (int i = 0; i < fields.Count; i++)
                                    if (fields[i].name.Equals("Approved")) fields[i] = ("Approved", false);
                        }
                    }
                    #endregion
                    return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Save(table, fields.ToArray()).ToString(), true);
                }
                else return new HttpResponse(HttpResponseCode.BadRequest, "Куда сохранять???", false);
            else return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на изменение данных!", false);
        }

        static HttpResponse RemoveProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
                if (query.TryGetByName("table", out string table))
                    if (query.TryGetByName("id", out int id))
                        return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Remove(table, id).ToString(), true);
                    else return new HttpResponse(HttpResponseCode.BadRequest, "Откуда удалять???", false);
                else return new HttpResponse(HttpResponseCode.BadRequest, "Что удалять???", false);
            else return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на удаление данных!", false);
        }

        static bool CheckAccess(IHttpHeaders query, Account account, bool write)
        {
            return account.AccountType > AccountType.Guest && account.Approved;
        }

        static bool CheckTableAccess(string tableName, Account account)
        {
            //TODO:Implement
            throw new NotImplementedException();
        }
        static bool CheckInTableAcess(string tableName, IHttpHeaders query, Account account)
        {
            //TODO:Implement
            throw new NotImplementedException();
        }
        #endregion
    }
}