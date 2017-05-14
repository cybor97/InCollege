using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class Group : DBRecord
    {
        public int SpecialtyID { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }

        [Ignore]
        [JsonIgnore]
        public string GroupInfo => $"{GroupName} {GroupCode}";

        [Ignore]
        [JsonIgnore]
        public Specialty Specialty
        {
            get => _specialty;
            set
            {
                _specialty = value;
                SpecialtyID = value?.ID ?? -1;
            }
        }
        private Specialty _specialty;

        [Ignore]
        [JsonIgnore]
        public string SpecialtyName { get => Specialty?.SpecialtyName; }
    }
}
