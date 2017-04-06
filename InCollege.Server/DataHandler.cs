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
            //FIXME:Change to POST
            if (request.Method == HttpMethods.Get)
                if (request.QueryString.TryGetByName("action", out string action))
                    context.Response = new HttpResponse(HttpResponseCode.Ok, Actions[action]?.Invoke(request.QueryString), false);
                else context.Response = new HttpResponse(HttpResponseCode.MethodNotAllowed, string.Empty, true);
            return Task.Factory.GetCompleted();
        }

        static Dictionary<string, Func<IHttpHeaders, string>> Actions = new Dictionary<string, Func<IHttpHeaders, string>>()
        {
            { "GetRange", GetRangeProcessor },
        };

        static string GetRangeProcessor(IHttpHeaders query)
        {
            int skip = query.TryGetByName("skipRecords", out int skipResult) ? skipResult : 0;
            int count = query.TryGetByName("countRecords", out int countResult) ? countResult : -1;
            string column = query.TryGetByName("column", out string columnResult) ? columnResult : null;
            bool fixedString = query.TryGetByName("fixedString", out bool fixedStringResult) && fixedStringResult;
            List<Tuple<string, object>> whereParams = new List<Tuple<string, object>>();
            foreach (var current in query)
                if (current.Key.StartsWith("where"))
                    whereParams.Add(new Tuple<string, object>(current.Key.Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries)[0], current.Value));
            return query.TryGetByName("table", out string table) ?
                JsonConvert.SerializeObject(DBHolderSQL.GetRange(table, column, skip, count, fixedString, whereParams.ToArray()), Formatting.Indented) :
                null;
        }
    }
}