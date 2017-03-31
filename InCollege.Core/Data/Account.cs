using InCollege.Core.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InCollege.Core.Data
{
    public class Account : DBRecord
    {
        public virtual int AccountDataID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public AccountType AccountType { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual byte[] ProfileImage { get; set; }

        public IEnumerable<Message> GetOutgoing(Message[] messages)
        {
            return messages.Where(m => m.FromID == ID);
        }

        public IEnumerable<Message> Inbox(Message[] messages)
        {
            return messages.Where(m => m.ToID == ID);
        }
    }
}
