using InCollege.Core.Data.Base;
using System;

namespace InCollege.Core.Data
{
    public class RePass : DBRecord
    {
        public int StatementResultID { get; set; }
        public int TeacherID { get; set; }
        public int RePassNumber { get; set; }
        public int MarkValue { get; set; }
        public DateTime RePassDate { get; set; }
    }
}
