using System.IO;

namespace InCollege
{
    public static class ClientConfiguration
    {
        //TODO:Implement correctly - through local configuration.
        public static string HostName { get; set; } = File.ReadAllText("host.txt");// "192.168.1.2";
        public static int Port { get; set; } = 80;

        public static readonly string AuthHandlerPath = $"http://{HostName}:{Port}/Auth";
        public static readonly string DataHandlerPath = $"http://{HostName}:{Port}/Data";
    }
}
