using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                    var validationResult = AuthorizationHandler.VerifyToken(tokenString);
                    if (validationResult.valid)
                        if (request.Post.Parsed.TryGetByName("action", out string action))
                            context.Response = Actions[action]?.Invoke(request.Post.Parsed, validationResult.account);
                        else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, "Эм.. что от меня требуется???", false);
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
            {
                int skip = query.TryGetByName("skipRecords", out int skipResult) ? skipResult : 0;
                int count = query.TryGetByName("countRecords", out int countResult) ? countResult : -1;
                string column = query.TryGetByName("column", out string columnResult) ? columnResult : null;
                bool fixedString = query.TryGetByName("fixedString", out bool fixedStringResult) && fixedStringResult;
                var whereParams = new List<(string, object)>();
                foreach (var current in query)
                    if (current.Key.StartsWith("where"))
                        whereParams.Add((current.Key.Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries)[0], current.Value));
                return new HttpResponse(HttpResponseCode.Ok,
                    query.TryGetByName("table", out string table) ?
                    JsonConvert.SerializeObject(DBHolderSQL.GetRange(table, column, skip, count, fixedString, whereParams.ToArray()), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) :
                    "Ошибка! Таблица не найдена!", false);
            }
            else return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на получение данных!", false);
        }

        static HttpResponse SaveProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
                if (query.TryGetByName("table", out string table))
                {
                    var fields = new List<(string, object)>();
                    foreach (var current in query)
                        if (current.Key.StartsWith("field"))
                            fields.Add((current.Key.Split(new[] { "field" }, StringSplitOptions.RemoveEmptyEntries)[0], current.Value));
                    return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Save(table, fields.ToArray()).ToString(), false);
                }
                else return new HttpResponse(HttpResponseCode.BadRequest, "Куда сохранять???", false);
            else return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на изменение данных!", false);
        }

        static HttpResponse RemoveProcessor(IHttpHeaders query, Account account)
        {
            if (CheckAccess(query, account, true))
                if (query.TryGetByName("table", out string table))
                    if (query.TryGetByName("id", out int id))
                        return new HttpResponse(HttpResponseCode.Ok, DBHolderSQL.Remove(table, id).ToString(), false);
                    else return new HttpResponse(HttpResponseCode.BadRequest, "Откуда удалять???", false);
                else return new HttpResponse(HttpResponseCode.BadRequest, "Что удалять???", false);
            else return new HttpResponse(HttpResponseCode.Forbidden, "У вас нет прав на удаление данных!", false);
        }

        static bool CheckAccess(IHttpHeaders query, Account account, bool write)
        {
            return account.AccountType > AccountType.Student && account.AccountDataID != -1 && account.Approved;
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