using InCollege.Core.Data.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class DayRecord :DBRecord
    {
        [Column("StudentID")]
        public virtual Student Student { get; set; }
        [Column("SubjectID")]
        public virtual Subject Subject { get; set; }
        public virtual DayRecordType RecordType { get; set; }
        public int MarkValue { get; set; }
        public virtual DateTime RecordDate { get; set; }
        public int Semester { get; set; }
        public string Note { get; set; }
    }
}
