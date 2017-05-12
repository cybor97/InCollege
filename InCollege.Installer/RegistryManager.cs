using Microsoft.Win32;
using System.Linq;

namespace InCollege.Installer
{
    class RegistryManager
    {
        public const string WINDOWS_APPS_SUBKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        public static void CreateApplicationEntry(AppPart part)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WINDOWS_APPS_SUBKEY, true))
                if (key.GetSubKeyNames().Count(c => c.Equals(part.ID)) == 0)
                    using (var CreatedKey = key.CreateSubKey(part.ID))
                        foreach (var current in part.Columns)
                            CreatedKey.SetValue(current.name, current.value);
                else
                {
                    RemoveApplicationEntry(part.ID);
                    CreateApplicationEntry(part);
                }
        }

        public static void RemoveApplicationEntry(string partID)
        {
            if (!string.IsNullOrWhiteSpace(partID))
                using (var key = Registry.LocalMachine.OpenSubKey(WINDOWS_APPS_SUBKEY, true))
                    key.DeleteSubKey(key.GetSubKeyNames().FirstOrDefault(c => c.Equals(partID)), false);
        }

        public static AppPart GetInstalledEntryInfo(string partName)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(WINDOWS_APPS_SUBKEY))
                if (key.GetSubKeyNames().Count(c => c.Equals(partName)) > 0)
                    using (var currentKey = key.OpenSubKey(partName))
                        return AppPart.ImportFromColumns(currentKey.GetValueNames().Select(c => (c, currentKey.GetValue(c))));
            return null;

        }
    }
}
