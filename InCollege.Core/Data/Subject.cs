using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class Subject : DBRecord
    {
        public string SubjectName { get; set; }
        public string SubjectIndex { get; set; }
    }
}
