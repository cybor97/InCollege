using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InCollege.Installer
{
    public class AppPart
    {
        //Yeah, string-id.
        public string ID { get; set; }
        public string ExecutablePath { get; set; }

        public string Contact { get; set; }
        public string DisplayIcon { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public bool NoModify { get; set; }
        public bool NoRepair { get; set; }
        public string Publisher { get; set; }
        public string InstallLocation { get; set; }
        public string UninstallString { get; set; }

        private AppPart() { }

        public IEnumerable<(string name, object value)> Columns =>
            GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(c => c.GetValue(this) != null)
            .Select(c =>
            {
                var value = c.GetValue(this);
                return (c.Name, (value is bool) ? (object)((bool)value ? 1 : 0) : value.ToString());
            });

        public static AppPart ImportFromColumns(IEnumerable<(string name, object value)> columns)
        {
            var result = new AppPart();
            foreach (var current in typeof(AppPart).GetProperties())
                current.SetValue(result, columns.FirstOrDefault(c => c.name == current.Name).value);
            return result;
        }
    }
}
