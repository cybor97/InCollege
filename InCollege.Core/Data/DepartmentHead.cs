using InCollege.Core.Data.Base;
using System;

namespace InCollege.Core.Data
{
    public class DepartmentHead:AccountData
    {
        public string FullName { get; set; }
        public virtual DateTime ApplyDate { get; set; }
    }
}
