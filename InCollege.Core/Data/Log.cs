using InCollege.Core.Data.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Core.Data
{
    public class Log : DBRecord
    {
        [Column("AccountID")]
        public virtual Account Account { get; set; }
        public virtual DateTime LogDate { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}
