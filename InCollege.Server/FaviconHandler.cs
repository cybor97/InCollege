using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using uhttpsharp;

namespace InCollege.Server
{
    internal class FaviconHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            using (var stream = new MemoryStream())
            {
                Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToBitmap().Save(stream, ImageFormat.Png);
                context.Response = new HttpResponse(HttpResponseCode.Ok, stream.GetBuffer(), true);
            }
            return Task.Factory.GetCompleted();
        }
    }
}