using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace InCollege.Server
{
    internal class HomeHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = new HttpResponse(HttpResponseCode.Ok,
                "<center><h1><b>Добро пожаловать</b></h1></center>" +
                "<center><h2><b>На сервер системы InCollege</b></h2></center>" +
                "<center><a href=\"/Auth\">Авторизация</a></center>" +
                "<center><a href=\"/Data?action=GetRange&table=AttestationType\">Данные</a></center>" +
                "<a style=\"position:fixed; bottom: 0; height: auto; margin - top:40px; width: 100 %; text - align:center\">Made by [CYBOR]</a>",
                true);

            return Task.Factory.GetCompleted();
        }
    }
}