using System;
using System.IO;

namespace InCollege.Server
{
    class CommonVariables
    {
        public static string DBLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InCollege.Server", "InCollege.db");
    }
}
