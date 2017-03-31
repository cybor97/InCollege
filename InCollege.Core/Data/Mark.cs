using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class Mark : DBRecord
    {
        public virtual int StudentID { get; set; }
        public virtual int SubjectID { get; set; }
        public int MarkValue { get; set; }
    }
}
