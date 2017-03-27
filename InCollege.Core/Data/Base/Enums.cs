namespace InCollege.Core.Data.Base
{
    public enum StatementType : byte
    {
        ExamStatement,
        MiddleStatement,
        TotalStatement
    }

    public enum AccountType : byte
    {
        Guest,
        Student,
        Professor,
        DepartmentHead,
        Admin
    }

    public enum DayRecordType:byte
    {
        AbsentRespectfulReason,
        Absent,
        Warning,
        Mark,
        MarkSeminar,
        MarkLabWork,
        MarkPracticalWork
    }
}
