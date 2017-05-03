using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;

namespace InCollege.Core.Data
{
    public class Subject : DBRecord
    {
        public string SubjectName { get; set; }
        public string SubjectIndex { get; set; }

        [Ignore]
        [JsonIgnore]
        public IEnumerable<Account> Teachers { get; set; }
    }
}
