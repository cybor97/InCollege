using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class CommissionMember : DBRecord
    {
        public int ProfessorID { get; set; }
        public int StatementID { get; set; }
        public string CommissionRole { get; set; }
    }
}
