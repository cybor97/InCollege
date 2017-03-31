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
                "<center><b>Добро пожаловать!<br/>Извините, но у нас нет веб-интерфейса...<br/>Тебя вообще здесь не должно быть!!!</b></center>", 
                false);
          
            return Task.Factory.GetCompleted();
        }
    }
}