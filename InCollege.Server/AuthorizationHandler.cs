using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace InCollege.Server
{
    class AuthorizationHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            return Task.Factory.GetCompleted();
        }
    }
}
