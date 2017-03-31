using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    public class Group : DBRecord
    {
        public int SpecialtyID { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
    }
}
