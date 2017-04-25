using InCollege.Core.Data.Base;
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
        public int Semester { get; set; }
        public virtual DateTime StatementDate { get; set; }
        public virtual string Note { get; set; }

        #region Display data
        [Ignore]
        public string StatementTypeString
        {
            get
            {
                return StatementTypeStrings[StatementType];
            }
        }
        #endregion

        #region Local data
        [Ignore]
        public List<AttestationType> AttestationTypes { get; set; }

        [Ignore]
        public List<CommissionMember> CommissionMembers { get; set; }

        //Either ExamStatementResult either MiddleStatementResult. Depends of StatementType.
        [Ignore]
        public List<DBRecord> StatementResults { get; set; }

        [Ignore]
        public Group Group { get; set; }

        [Ignore]
        public Subject Subject { get; set; }
        #endregion

        #region Service data
        static readonly Dictionary<StatementType, string> StatementTypeStrings = new Dictionary<StatementType, string>()
        {
            { StatementType.Middle, "Промежуточная" },
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
