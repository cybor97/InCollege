namespace InCollege
{
    public static class ClientConfiguration
    {
        public static string HostName { get; set; } = "localhost";
        public static int Port { get; set; } = 80;

        public static readonly string AuthHandlerPath = $"http://{HostName}:{Port}/Auth";
        public static readonly string DataHandlerPath = $"http://{HostName}:{Port}/Data";
    }
}
