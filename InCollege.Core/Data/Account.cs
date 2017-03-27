using InCollege.Core.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace InCollege.Core.Data
{
    public class Account : DBRecord
    {
        [Column("AccountDataID")]
        public virtual AccountData AccountData { get; set; }
        [StringLength(50)]
        public string UserName { get; set; }
        [StringLength(50)]
        public string Password { get; set; }
        public virtual AccountType AccountType { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual byte[] ProfileImage { get; set; }

        public virtual List<Log> Logs { get; set; }
        public virtual List<Message> Messages { get; set; }

        [NotMapped]
        public IEnumerable<Message> Outgoing { get => Messages.Where(m => m.From == this); }
        [NotMapped]
        public IEnumerable<Message> Inbox { get => Messages.Where(m => m.To == this); }
    }
}
