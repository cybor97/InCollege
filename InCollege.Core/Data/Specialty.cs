using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Specialty : DBRecord
    {
        [Column("DepartmentID")]
        public virtual Department Department { get; set; }
        public string SpecialtyName { get; set; }
        public string SpecialtyCode { get; set; }
    }
}
