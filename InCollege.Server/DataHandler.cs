using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System;
using System.Data.OleDb;
using System.Threading.Tasks;
using uhttpsharp;

namespace InCollege.Server
{
    internal class DataHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            string response = "";
            using (var db = new DBHolder(new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\muham\\Desktop\\test.accdb")))
            {
                if (context.Request.QueryString.TryGetByName("addAccount", out string userName))
                    db.Accounts.Add(new Account() { UserName = userName });
                if (context.Request.QueryString.TryGetByName("accounts", out userName) && userName != null)
                    foreach (var current in db.Accounts)
                        response += current.UserName + "\n";
            }
            context.Response = new HttpResponse(HttpResponseCode.Ok, response, false);
            return Task.Factory.GetCompleted();
        }
    }
}