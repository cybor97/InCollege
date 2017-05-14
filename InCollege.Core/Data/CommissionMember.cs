using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class CommissionMember : DBRecord
    {
        public int ProfessorID { get; set; }
        public int StatementID { get; set; }
        public string CommissionRole { get; set; }

        [Ignore]
        [JsonIgnore]
        public Account Professor
        {
            get => _professor;
            set
            {
                _professor = value;
                ProfessorID = value?.ID ?? -1;
            }
        }
        private Account _professor;

        [Ignore]
        [JsonIgnore]
        public string FullName => Professor?.FullName;
    }
}
