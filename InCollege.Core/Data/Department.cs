using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Department : DBRecord
    {
        [Column("DepartmentHeadID")]
        public virtual DepartmentHead DepartmentHead { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
    }
}
