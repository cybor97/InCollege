using System;
using System.IO;

namespace InCollege.Installer
{
    public class CommonVariables
    {
        public static string InstallRoot = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        public static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string StartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        public static string StartUpPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static string TMPDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InCollege.TMP");
    }
}
