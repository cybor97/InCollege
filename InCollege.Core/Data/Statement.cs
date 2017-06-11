using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace InCollege.Core.Data
{
    public class Statement : DBRecord
    {
        public int SubjectID { get; set; }
        public int GroupID { get; set; }
        public StatementType StatementType { get; set; }
        public int StatementNumber { get; set; }
        public int Course { get; set; }
        public int Semester { get; set; }
        public DateTime StatementDate { get; set; } = DateTime.Now;
        public string Note { get; set; }

        #region Display data
        [Ignore]
        [JsonIgnore]
        public string StatementTypeString => StatementTypeStrings[StatementType];
        #endregion

        #region Local data
        [Ignore]
        [JsonIgnore]
        public Specialty Specialty { get; set; }

        [Ignore]
        [JsonIgnore]
        public Group Group
        {
            get => _group;
            set
            {
                _group = value;
                GroupID = value?.ID ?? -1;
            }
        }
        private Group _group;

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
        #endregion

        #region Service data
        static readonly Dictionary<StatementType, string> StatementTypeStrings = new Dictionary<StatementType, string>()
        {
            { StatementType.Middle, "Зачетная" },
            { StatementType.Exam, "Экзаменационная" },
            { StatementType.QualificationExam, "Протокол квалификационного экзамена" },
            { StatementType.StudyPractice, "Учебная практика" },
            { StatementType.IndustrialPractice, "Производственная практика" },
            { StatementType.CourceProject, "Курсовой проект" },
            { StatementType.Total, "Сводная" },
            { StatementType.Other, "Прочее" }
        };
        #endregion
    }
}
