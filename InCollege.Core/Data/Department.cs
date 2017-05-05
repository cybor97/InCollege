using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class Department : DBRecord
    {
        public int DepartmentHeadID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }


        [Ignore]
        [JsonIgnore]
        public Account DepartmentHead { get; set; }

        [Ignore]
        [JsonIgnore]
        public string DepartmentHeadName { get => DepartmentHead?.FullName; }
    }
}
