using InCollege.Core.Data.Base;
using SQLite;
using System;

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

        [Ignore]
        public string StatementTypeString
        {
            get
            {
                switch (StatementType)
                {
                    case StatementType.MiddleStatement:
                        return "Промежуточная ведомость";
                    case StatementType.ExamStatement:
                        return "Экзаменационная ведомость";
                    case StatementType.TotalStatement:
                        return "Сводная ведомость";
                    default:
                        return "Не определено :(";
                }
            }
        }
    }
}
