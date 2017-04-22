using System;
using System.IO;

namespace InCollege
{
    class CommonVariables
    {
        public const string APP_NAME = "InCollege";
        public static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
        public static readonly string DBFileName = Path.Combine(DataDirectory, "InCollege.db");
        public static readonly string TokenFileName = Path.Combine(DataDirectory, "Account.token");
    }
}
