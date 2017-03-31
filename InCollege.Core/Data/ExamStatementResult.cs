using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class ExamStatementResult:DBRecord
    {
        public int ExamStatementID { get; set; }
        public int MarkID { get; set; }
        public int TicketNumber { get; set; }
    }
}
