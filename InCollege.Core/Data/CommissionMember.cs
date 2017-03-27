using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class CommissionMember : DBRecord
    {
        [Column("ProfessorID")]
        public virtual Professor Professor { get; set; }
        [Column("StatementID")]
        public virtual Statement Statement { get; set; }
        [StringLength(255)]
        public string CommissionRole { get; set; }
    }
}
