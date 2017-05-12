using System;

namespace InCollege.Installer
{
    public class CommonVariables
    {
        public static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string StartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        public static string StartUpPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    }
}
