using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class Specialty : DBRecord
    {
        public int DepartmentID { get; set; }
        public string SpecialtyName { get; set; }
        public string SpecialtyCode { get; set; }
    }
}
