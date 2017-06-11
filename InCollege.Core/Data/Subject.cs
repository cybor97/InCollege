using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class Subject : DBRecord
    {
        public int SpecialtyID { get; set; } = -1;
        public int Semester { get; set; } = 1;
        public string SubjectName { get; set; }
        public string SubjectIndex { get; set; }

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
    }
}
