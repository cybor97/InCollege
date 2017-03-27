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
                "<center><b>Добро пожаловать!\nИзвините, но у нас нет веб-интерфейса...</b></center>", 
                false);
          
            return Task.Factory.GetCompleted();
        }
    }
}