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
                        if ((DateTime.Now.Subtract(validationResult.account.LastAction ?? new DateTime()).TotalSeconds) > Account.OnlineTimeoutSeconds - 1)
                        {
                            validationResult.account.LastAction = DateTime.Now;
                            DBHolderSQL.Save(nameof(Account), (nameof(Account.ID), validationResult.account.ID), (nameof(Account.LastAction), validationResult.account.LastAction));
                        }
                        if (request.Post.Parsed.TryGetByName("action", out string action))
                            context.Response = Actions[action]?.Invoke(request.Post.Parsed, validationResult.account);
                        else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Эм.. что от меня требуется???", false);
                    }
                    else
                    {
                        DBHolderSQL.Log($"[ОШИБКА ДОСТУПА] Пользователь с поврежденным или подделанным токеном пытался войти в систему. Экземпляр токена предоставлен в описании.",
                                        $"{tokenString}");
                        context.Response = new HttpResponse(HttpResponseCode.Forbidden, "Доступ запрещен! Ошибка разбора токена!", false);
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
            { "Remove", RemoveProcessor },
            { "RemoveWhere", RemoveWhereProcessor},
            { "Chat", ChatProcessor },
            { "Disconnect", DisconnectProcessor }
        };

        static HttpResponse DisconnectProcessor(IHttpHeaders query, Account account)
        {
            return new HttpResponse(HttpResponseCode.Ok, string.Empty, false);
        }

        static HttpResponse ChatProcessor(IHttpHeaders query, Account account)
        {
            if (account.Approved)
            {
                if (query.TryGetByName("mode", out byte mode))
                {
                    switch (mode)
                    {
                        case (byte)ChatRequestMode.CheckMessages:

                            (string, object)[] whereParams;
                            if (query.TryGetByName("partnerID", out int partnerID))
                                whereParams = new[] { (nameof(Message.FromID), (object)partnerID), (nameof(Message.ToID), account.ID), (nameof(Message.IsRead), false) };
                            else whereParams = new[] { (nameof(Message.ToID), (object)account.ID), (nameof(Message.IsRead), false) };

                            return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.GetRange(nameof(Message), null, -1, -1, true, true, false, false, whereParams)
                                        .Rows?[0]?[0].ToString(), true);

                        case (byte)ChatRequestMode.Conversation:
                            if (query.TryGetByName("partnerID", out partnerID))
                            {
                                var range = DBHolderSQL.GetRange(nameof(Message), null, -1, -1, true, false, false, false,
                                                                (nameof(Message.FromID), partnerID),
                                                                (nameof(Message.ToID), account.ID));
                                range.Merge(DBHolderSQL.GetRange(nameof(Message), null, -1, -1, true, false, false, false,
                                                                (nameof(Message.FromID), account.ID),
                                                                (nameof(Message.ToID), partnerID)));

                                foreach (DataRow current in range.Rows)
                                    if ((current[nameof(Message.IsRead)] == DBNull.Value || (long)current[nameof(Message.IsRead)] == 0) && (long)current[nameof(Message.FromID)] != account.ID)
                                        DBHolderSQL.Save(nameof(Message), ("ID", current["ID"]), (nameof(Message.IsRead), 1));

                                return new HttpResponse(HttpResponseCode.Ok, JsonConvert.SerializeObject(range, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), true);
                            }
                            else return new HttpResponse(HttpResponseCode.BadRequest, "Укажите partnerID!", false);

                        case (byte)ChatRequestMode.Friends:
                            return new HttpResponse(HttpResponseCode.Ok, JsonConvert.SerializeObject(DBHolderSQL.GetFriends(account.ID)), true);

                        case (byte)ChatRequestMode.Send:
                            var fields = query
                                .Where(c => c.Key != nameof(Message.FromID) && c.Key != "Action" && c.Key != "token" && c.Key != "mode" && c.Key != "partnerID" && c.Key != "table")
                                .Select(c => (name: c.Key, value: (object)c.Value))
                                .ToList();
                            fields.Add((nameof(Message.FromID), account.ID));
                            return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Save(nameof(Message), fields.ToArray()).ToString(), true);
                    }
                }
                return new HttpResponse(HttpResponseCode.BadRequest, "Ошибка! Режим не определен.", false);
            }
            else return new HttpResponse(HttpResponseCode.Forbidden, "Аккаунт не подтвержден, отправка сообщений невозможна!", false);
        }
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
                    bool orAll = query.TryGetByName("orAll", out int orAllResult) && orAllResult == 1;
                    var whereParams = query.Where(c => c.Key.StartsWith("where"))
                                           .Select(c => (name: c.Key.Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries)[0], value: (object)c.Value));

                    var range = DBHolderSQL.GetRange(table, column, skip, count, fixedString, justCount, reverse, orAll, whereParams.ToArray());

                    #region Special rules for accounts
                    //We never should send passwords...
                    //...but if it's just count requested - why not?
                    if (table == nameof(Account) && !justCount && range.Columns.Contains(nameof(Account.Password)))
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
                                DBHolderSQL.Save(table, ("ID", current["ID"]), (nameof(Message.IsRead), 1));
                    #endregion

                    if (!justCount)
                        return new HttpResponse(HttpResponseCode.Ok, JsonConvert.SerializeObject(range, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), true);
                    else return new HttpResponse(HttpResponseCode.Ok, range.Rows[0][0].ToString(), true);
                }
                else return new HttpResponse(HttpResponseCode.BadRequest, "Ошибка! Таблица не найдена!", false);
            else
            {
                DBHolderSQL.Log($"[ОШИБКА ДОСТУПА] Пользователь с неподходящим уровнем доступа пытается получить данные.",
                                $"ФИО:{account?.FullName}\nИмя пользователя(логин):{account?.UserName}\nТип аккаунта:{account?.AccountTypeString}\n" +
                                $"Текст запроса:{query.ToUriData()}", account?.ID ?? -1);
                return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на получение данных!", false);
            }
        }

        static HttpResponse SaveProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
                if (query.TryGetByName("table", out string table))
                {
                    var fields = query.Where(c => c.Key.StartsWith("field")).Select(c => (name: c.Key.Split(new[] { "field" }, StringSplitOptions.RemoveEmptyEntries)[0], value: (object)c.Value)).ToArray();

                    #region Special rules for accounts. Need to be optimized.
                    if (table.Equals(nameof(Account)))
                    {
                        //Empty password field means that user don't want to change password.
                        for (int i = 0; i < fields.Length; i++)
                            if (fields[i].name.Equals("Password") && string.IsNullOrWhiteSpace((string)fields[i].value))
                                fields[i] = ("Password", account.Password);

                        //Only admin can edit foreign accounts
                        //Aand admin can do really anything with them
                        if (account.AccountType != AccountType.Admin || !account.Approved)
                        {
                            if (account.ID != int.Parse((string)fields.FirstOrDefault(c => c.name == "ID").value))
                                return new HttpResponse(HttpResponseCode.Forbidden, "Вы не можете редактировать данные другого аккаунта!", false);

                            for (int i = 0; i < fields.Length; i++)
                                if (fields[i].name.Equals("Approved")) fields[i] = ("Approved", account.Approved);

                            //Account shouldn't be approved after AccountType switch.
                            if (query.TryGetByName("fieldAccountType", out byte accountType) &&
                                accountType != (byte)account.AccountType)
                                for (int i = 0; i < fields.Length; i++)
                                    if (fields[i].name.Equals("Approved")) fields[i] = ("Approved", false);
                        }
                    }
                    #endregion
                    return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Save(table, fields.ToArray()).ToString(), true);
                }
                else return new HttpResponse(HttpResponseCode.BadRequest, "Куда сохранять???", false);
            else
            {
                DBHolderSQL.Log($"[ОШИБКА ДОСТУПА] Пользователь с неподходящим уровнем доступа пытается изменить данные.",
                                $"ФИО:{account?.FullName}\nИмя пользователя(логин):{account?.UserName}\nТип аккаунта:{account?.AccountTypeString}\n" +
                                $"Текст запроса:{query.ToUriData()}", account?.ID ?? -1);
                return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на изменение данных!", false);
            }
        }

        static HttpResponse RemoveProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
                if (query.TryGetByName("table", out string table))
                    if (query.TryGetByName("id", out int id))
                        return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Remove(table, id).ToString(), true);
                    else return new HttpResponse(HttpResponseCode.BadRequest, "Откуда удалять???", false);
                else return new HttpResponse(HttpResponseCode.BadRequest, "Что удалять???", false);
            else
            {
                DBHolderSQL.Log($"[ОШИБКА ДОСТУПА] Пользователь с неподходящим уровнем доступа пытается удалить данные.",
                                $"ФИО:{account?.FullName}\nИмя пользователя(логин):{account?.UserName}\nТип аккаунта:{account?.AccountTypeString}\n" +
                                $"Текст запроса:{query.ToUriData()}", account?.ID ?? -1);
                return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на удаление данных!", false);
            }
        }

        static HttpResponse RemoveWhereProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
            {
                if (query.TryGetByName("table", out string table))
                {
                    var whereParams = new List<(string, object)>();

                    foreach (var current in query)
                        if (current.Key.StartsWith("where"))
                            if (!string.IsNullOrWhiteSpace(current.Key))
                                whereParams.Add((current.Key.Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries)[0], current.Value));

                    if (whereParams.Count > 0)
                        return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.RemoveWhere(table, whereParams.ToArray()).ToString(), true);
                }
                return new HttpResponse(HttpResponseCode.BadRequest, "Что удалять???", false);
            }
            else
            {
                DBHolderSQL.Log($"[ОШИБКА ДОСТУПА] Пользователь с неподходящим уровнем доступа пытается МАССОВО удалить данные.",
                                $"ФИО:{account?.FullName}\nИмя пользователя(логин):{account?.UserName}\nТип аккаунта:{account?.AccountTypeString}\n" +
                                $"Текст запроса:{query.ToUriData()}", account?.ID ?? -1);
                return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на удаление данных!", false);
            }
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