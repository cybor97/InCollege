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
        public string SpecialtyName { get; set; }
    }
}
