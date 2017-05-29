using System.Collections.Generic;
using System;
using SQLite;
using System.IO;

namespace InCollege.Core.Data.Base
{
    public static class DBHolderORM
    {
        static SQLiteConnection DataConnection;
        public const int DEFAULT_LIMIT = int.MaxValue / 2;
        static bool InitCompleted;

        public static void Init(string filename)
        {
            if (!InitCompleted)
            {
                DirectoryInfo parentDirectory;
                if (!Directory.Exists((parentDirectory = Directory.GetParent(filename)).FullName))
                    Directory.CreateDirectory(parentDirectory.FullName);

                DataConnection = new SQLiteConnection(filename);

                CreateTables(
                    typeof(Account),
                    typeof(AttestationType),
                    typeof(ConfigurationParameter),
                    typeof(Department),
                    typeof(Group),
                    typeof(Log),
                    typeof(Message),
                    typeof(StatementResult),
                    typeof(CommissionMember),
                    typeof(Specialty),
                    typeof(Statement),
                    typeof(RePass),
                    typeof(StatementAttestationType),
                    typeof(Subject),
                    typeof(Teacher));
                InitCompleted = true;
                DataConnection.Close();
            }
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

        public static void Save(DBRecord record)
        {
            DataConnection.InsertOrReplace(record);
        }

        public static void Remove(DBRecord record)
        {
            DataConnection.Delete(record);
        }
    }
}
