using InCollege.Core.Data.Base;
using System;

namespace InCollege.Core.Data
{
    public class Statement:DBRecord
    {
        public int SubjectID { get; set; }
        public int GroupID { get; set; }
        public StatementType StatementType { get; set; }
        public int StatementNumber { get; set; }
        public int Semester { get; set; }
        public virtual DateTime StatementDate { get; set; }
        public virtual string Note { get; set; }
    }
}
