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
        public static bool Working { get => Server != null; }

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
                .With("SignIn", new SignInHandler())
                .With("SignUp", new SignUpHandler())
                .With("Data", new DataHandler()));

                Server.Start();
            }
        }
    }
}
