using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class ExamStatementResult : DBRecord
    {
        public int ExamStatementID { get; set; }
        public int StudentID { get; set; }
        public int MarkValue { get; set; } = 2;
        public int TicketNumber { get; set; }
    }
}
