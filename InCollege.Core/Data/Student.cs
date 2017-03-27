using InCollege.Core.Data.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Student : AccountData
    {
        [Column("GroupID")]
        public virtual Group Group { get; set; }
        public string FullName { get; set; }
    }
}
