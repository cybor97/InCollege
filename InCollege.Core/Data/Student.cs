using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class Student : AccountData
    {
        public int GroupID { get; set; }
        public string FullName { get; set; }
    }
}
