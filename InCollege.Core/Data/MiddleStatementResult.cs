using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class MiddleStatementResult : DBRecord
    {
        public int MiddleStatementID { get; set; }
        public int QualificationStatementID { get; set; }
        public int DayRecordID { get; set; }
    }
}
