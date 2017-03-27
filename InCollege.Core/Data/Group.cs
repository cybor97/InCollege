using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Group:DBRecord
    {
        [Column("SpecialtyID")]
        public virtual Specialty Specialty { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
    }
}
