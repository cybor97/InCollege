using System.Collections.Generic;
using System;
using SQLite;

namespace InCollege.Core.Data.Base
{
    public static class DBHolderORM
    {
        static SQLiteConnection DataConnection;
        public const int DEFAULT_LIMIT = 100;
        public static void Init(string filename)
        {
            DataConnection = new SQLiteConnection(filename);
            CreateTables(
                typeof(Account),
                typeof(AttestationType),
                typeof(ConfigurationParameter),
                typeof(DepartmentHead),
                typeof(Department),
                typeof(ExamStatementResult),
                typeof(Group),
                typeof(Log),
                typeof(Mark),
                typeof(Message),
                typeof(MiddleStatementResult),
                typeof(Professor),
                typeof(CommissionMember),
                typeof(Specialty),
                typeof(Statement),
                typeof(Student),
                typeof(Subject));
        }

        public static List<T> GetAll<T>() where T : DBRecord, new()
        {
            return GetRange<T>(0, DEFAULT_LIMIT);
        }

        static void CreateTables(params Type[] tables)
        {
            foreach (var current in tables)
                DataConnection.CreateTable(current);
        }

        public static List<T> GetRange<T>(int skip, int count) where T : DBRecord, new()
        {
            return DataConnection.Query<T>(string.Format("SELECT * FROM {0} LIMIT {1}, {2}", typeof(T).Name, skip, count));
        }

        public static void Save(AttestationType record)
        {
            DataConnection.InsertOrReplace(record);
        }

        public static void Remove(AttestationType record)
        {
            DataConnection.Delete(record);
        }
    }
}
