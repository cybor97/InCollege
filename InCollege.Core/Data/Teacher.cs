using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class Teacher : DBRecord
    {
        public int ProfessorID { get; set; }
        public int SubjectID { get; set; }
    }
}
