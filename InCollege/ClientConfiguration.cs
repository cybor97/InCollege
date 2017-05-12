﻿using Newtonsoft.Json;
using System.IO;

namespace InCollege
{
    public class ClientConfiguration
    {
        public static ClientConfiguration Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                if (File.Exists(CommonVariables.ConfigFileName))
                    return _instance = JsonConvert.DeserializeObject<ClientConfiguration>(File.ReadAllText(CommonVariables.ConfigFileName));

                File.WriteAllText(CommonVariables.ConfigFileName, JsonConvert.SerializeObject(_instance = new ClientConfiguration()));
                return _instance;
            }
        }
        private static ClientConfiguration _instance;

        public string HostName { get; set; } = File.Exists("host.txt") ? File.ReadAllText("host.txt") : "127.0.0.1";
        public int Port { get; set; } = 80;

        public string AuthHandlerPath => $"http://{HostName}:{Port}/Auth";
        public string DataHandlerPath => $"http://{HostName}:{Port}/Data";
    }
}
