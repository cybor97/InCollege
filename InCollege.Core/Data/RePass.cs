using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System;

namespace InCollege.Core.Data
{
    public class RePass : DBRecord
    {
        public int StatementID { get; set; }
        public int StatementResultID { get; set; }
        public int TeacherID { get; set; }
        public int RePassNumber { get; set; }
        public sbyte MarkValue { get; set; }
        public DateTime RePassDate { get; set; }

        [Ignore]
        [JsonIgnore]
        public string MarkValueString
        {
            get => StatementResult.GetMarkValueString(MarkValue);
        }

        [Ignore]
        [JsonIgnore]
        public Teacher Teacher
        {
            get => _teacher;
            set
            {
                _teacher = value;
                TeacherID = value?.ID ?? -1;
            }
        }
        private Teacher _teacher;
    }
}
