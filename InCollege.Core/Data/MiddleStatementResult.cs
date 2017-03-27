using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class MiddleStatementResult : DBRecord
    {
        [Column("MiddleStatementID")]
        public virtual Statement MiddleStatement { get; set; }
        [Column("QualificationStatementID")]
        public virtual Statement QualificationStatement { get; set; }
        [Column("DayRecordID")]
        public virtual DayRecord DayRecord { get; set; }
    }
}
