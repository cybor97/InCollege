using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace InCollege.Server
{
    internal class SignUpHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            if (context.Request.Method == HttpMethods.Get)
                context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.Forbidden, "Unsafe method!", false);
            else if (context.Request.Method == HttpMethods.Post)
                context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotImplemented, "Sorry, still unimplemented!", false);
            return Task.Factory.GetCompleted();
        }
    }
}