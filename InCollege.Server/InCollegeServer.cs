using System;
using System.Net;
using System.Net.Sockets;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace InCollege.Server
{
    internal static class InCollegeServer
    {
        private static HttpServer Server;
        public static bool Working => Server != null;

        public static void Start()
        {
            if (!Working)
            {
                Server = new HttpServer(new HttpRequestProvider());
                
                Server.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Any, 80)));
                //  Server.Use(new ListenerSslDecorator(new TcpListenerAdapter(new TcpListener(IPAddress.Any, 443)),
                //    X509Certificate.CreateFromCertFile("")));
                Server.Use(new HttpRouter()
                .With(String.Empty, new HomeHandler())
                .With("Auth", new AuthorizationHandler())
                .With("Data", new DataHandler())
                .With("favicon.ico", new FaviconHandler()));

                Server.Start();
            }
        }
    }
}
