using InCollege.Core.Data.Base;
using System;

namespace InCollege.Core.Data
{
    public class Log : DBRecord
    {
        public virtual int AccountID { get; set; }
        public virtual DateTime LogDate { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}
