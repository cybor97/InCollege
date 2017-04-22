using InCollege.Core.Data.Base;

namespace InCollege.Core.Data
{
    class StatementAttestationType:DBRecord
    {
        public int AttestationTypeID { get; set; }
        public int StatementID { get; set; }
    }
}
