using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace InCollege.Server
{
    internal class HomeHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            if (context.Request.QueryString.TryGetByName("CheckVersion", out string value) && value == "1")
                context.Response = new HttpResponse(HttpResponseCode.Ok, CommonVariables.Version, true);
            else if (context.Request.QueryString.TryGetByName("assign", out value) && value == "1")
                context.Response = new HttpResponse(HttpResponseCode.Ok, "Молодец! 2-я и последняя пасхалка! Больше нет! 2014, 18, 3, oneword", true);
            else if (context.Request.QueryString.TryGetByName("referendum", out value) && value == "1")
                context.Response = new HttpResponse(HttpResponseCode.Ok, "Экзамен по истории сдан! Или не сдан?.. Зависит от точки зрения.", true);
            else if (context.Request.QueryString.TryGetByName("view", out value) && value == "1")
                context.Response = new HttpResponse(HttpResponseCode.Ok, "Да сколько можно??? undefined", true);
            else if (context.Request.QueryString.TryGetByName("Auth", out value) && value == "1")
                context.Response = new HttpResponse(HttpResponseCode.Ok, Properties.Resources.AuthPage, true);
            else if (context.Request.QueryString.TryGetByName("Data", out value) && value == "1")
                context.Response = new HttpResponse(HttpResponseCode.Ok, Properties.Resources.DataPage, true);
            else
                context.Response = new HttpResponse(HttpResponseCode.Ok, Properties.Resources.HomePage.Replace("{Version}", CommonVariables.Version), true);

            return Task.Factory.GetCompleted();
        }
    }
}