using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using SQLite;

namespace InCollege.Core.Data
{
    public class StatementAttestationType : DBRecord
    {
        public int AttestationTypeID { get; set; }
        public int StatementID { get; set; }

        [Ignore]
        [JsonIgnore]
        public AttestationType AttestationType
        {
            get => _attestationType;
            set
            {
                _attestationType = value;
                AttestationTypeID = value?.ID ?? -1;
            }
        }
        private AttestationType _attestationType;

        public string TypeName => AttestationType?.TypeName;
    }
}
