using InCollege.Core.Data.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Statement:DBRecord
    {
        [Column("SubjectID")]
        public virtual Subject Subject { get; set; }
        [Column("GroupID")]
        public virtual Group Group { get; set; }
        public StatementType StatementType { get; set; }
        public int StatementNumber { get; set; }
        public int Semester { get; set; }
        public virtual DateTime StatementDate { get; set; }
        public virtual string Note { get; set; }
    }
}
