using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class StatementResult : DBRecord
    {
        public int StatementID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public sbyte MarkValue { get; set; } = 2;
        public int TicketNumber { get; set; } = -1;

        public string MarkValueString
        {
            get
            {
                switch (MarkValue)
                {
                    case (sbyte)TechnicalMarkValue.Absent:
                        return "Н";
                    case (sbyte)TechnicalMarkValue.Passed:
                        return "Зачтено";
                    case (sbyte)TechnicalMarkValue.Unpassed:
                        return "Не зачтено";
                    case (sbyte)TechnicalMarkValue.Blank:
                        return string.Empty;
                    default: return MarkValue.ToString();
                }
            }
        }

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

        [Ignore]
        [JsonIgnore]
        public string SubjectName => Subject?.SubjectName;

        [Ignore]
        [JsonIgnore]
        public string SubjectIndex => Subject?.SubjectIndex;

        [Ignore]
        [JsonIgnore]
        public Subject Subject
        {
            get => _subject;
            set
            {
                _subject = value;
                SubjectID = value?.ID ?? -1;
            }
        }
        private Subject _subject;
    }
}
