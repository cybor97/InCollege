using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class Specialty : DBRecord
    {
        public int DepartmentID { get; set; }
        public string SpecialtyName { get; set; }
        public string SpecialtyCode { get; set; }

        [Ignore]
        [JsonIgnore]
        public Department Department
        {
            get => _department;
            set
            {
                _department = value;
                DepartmentID = value?.ID ?? -1;
            }
        }
        private Department _department;

        [Ignore]
        [JsonIgnore]
        public string DepartmentName { get => Department?.DepartmentName; }
    }
}
