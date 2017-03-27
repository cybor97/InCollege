using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class ExamStatementResult:DBRecord
    {
        [Column("ExamStatementID")]
        public virtual Statement ExamStatement { get; set; }
        [Column("DayRecordID")]
        public virtual DayRecord DayRecord { get; set; }
        public int TicketNumber { get; set; }
    }
}
