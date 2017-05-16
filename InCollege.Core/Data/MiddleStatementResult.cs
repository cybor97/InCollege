using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class MiddleStatementResult : DBRecord
    {
        public int MiddleStatementID { get; set; }
        public int QualificationStatementID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int MarkValue { get; set; }


        [Ignore]
        [JsonIgnore]
        public string StudentFullName => Student?.FullName;

        [Ignore]
        [JsonIgnore]
        public Account Student
        {
            get => _student;
            set
            {
                _student = value;
                StudentID = value?.ID ?? -1;
            }
        }
        private Account _student;
    }
}
