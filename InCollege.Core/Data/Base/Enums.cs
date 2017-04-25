namespace InCollege.Core.Data.Base
{
    public enum StatementType : byte
    {
        Middle,
        Exam,
        QualificationExam,
        StudyPractice,
        IndustrialPractice,
        CourceProject,
        Total,
        Other
    }

    public enum AccountType : byte
    {
        Guest = 1,
        Student = 2,
        Professor = 3,
        DepartmentHead = 4,
        Admin = 5
    }

    public enum TechnicalMarkValue : sbyte
    {
        Absent = -1,

    }
}
