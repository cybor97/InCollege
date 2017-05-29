using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System;

namespace InCollege.Core.Data
{
    public class StatementResult : DBRecord
    {
        public int StatementID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public sbyte MarkValue { get; set; } = 2;
        public int TicketNumber { get; set; } = -1;
        public DateTime? StatementResultDate { get; set; }

        [Ignore]
        [JsonIgnore]
        public string StatementResultDateString => StatementResultDate?.ToString("dd MMMM yyyy") ?? "";

        [Ignore]
        [JsonIgnore]
        public string MarkValueString => GetMarkValueString(MarkValue);

        [Ignore]
        [JsonIgnore]
        public string MarkValueStringTotal => GetMarkValueStringTotal(MarkValue);

        public static string GetMarkValueStringTotal(sbyte markValue)
        {
            switch (markValue)
            {
                case (sbyte)TechnicalMarkValue.Absent:
                    return "Н";
                case (sbyte)TechnicalMarkValue.Passed:
                    return "[З]";
                case (sbyte)TechnicalMarkValue.Unpassed:
                    return "Н/А";
                case (sbyte)TechnicalMarkValue.Blank:
                    return string.Empty;
                default: return markValue.ToString();
            }
        }

        public static string GetMarkValueString(sbyte markValue)
        {
            switch (markValue)
            {
                case (sbyte)TechnicalMarkValue.Absent:
                    return "Не явился";
                case (sbyte)TechnicalMarkValue.Passed:
                    return "Зачтено";
                case (sbyte)TechnicalMarkValue.Unpassed:
                    return "Не аттестован";
                case (sbyte)TechnicalMarkValue.Blank:
                    return string.Empty;
                default: return markValue.ToString();
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

        //TODO:It's cludge. Fix me, please!!!
        [Ignore]
        public string SubjectName_STUDENT_MODE { get; set; }
    }
}
