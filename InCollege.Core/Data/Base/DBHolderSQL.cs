using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace InCollege.Core.Data.Base
{
    /// <summary>
    /// DO NOT USE ON CLIENT SIDE!!!11
    /// </summary>
    public static class DBHolderSQL
    {
        static SQLiteConnection DataConnection;
        static Dictionary<string, SQLiteDataAdapter> Adapters;
        public static void Init(string filename)
        {
            //Create required tables and... at all, prepare db.
            //I hope this isn't shit-code-design.
            DBHolderORM.Init(filename);

            DataConnection = new SQLiteConnection($"Data Source={filename}; Version=3;");
            DataConnection.Open();
            Adapters = new Dictionary<string, SQLiteDataAdapter>();

            foreach (DataRow current in DataConnection.GetSchema("Tables").Rows)
                Adapters.Add(current[2].ToString(), new SQLiteDataAdapter("SELECT * FROM {0} LIMIT {1}, {2}", DataConnection));
        }

        public static DataTable GetRange(string table, string column, int skip, int count, bool fixedString, params (string name, object value)[] whereParams)
        {
            if (count == -1)
                count = DBHolderORM.DEFAULT_LIMIT;
            if (table == null)
                table = "master_table";
            if (string.IsNullOrWhiteSpace(column))
                column = "*";
            DataTable result = new DataTable(table);
            string whereString = "";
            if (whereParams != null)
                for (int i = 0; i < whereParams.Length; i++)
                {
                    object name = whereParams[i].name,
                        value = whereParams[i].value;
                    whereString += value is string ? fixedString ?
                        $"{name} LIKE '{value}'" :
                        $"instr({name}, '{value}') > 0" :
                        $"{name} LIKE {value}";
                    if (i < whereParams.Length - 1)
                        whereString += " AND ";
                }
            Adapters[table].SelectCommand =
                new SQLiteCommand($"SELECT {column} FROM {table} " +
                                  (string.IsNullOrWhiteSpace(whereString) ? $"" : $"WHERE {whereString} ") +
                                  $"LIMIT {skip}, {count} ",
                    DataConnection);
            Adapters[table].Fill(result);
            return result;
        }

        public static DataRow GetByID(string table, int id)
        {
            DataTable result = new DataTable();
            Adapters[table].SelectCommand = new SQLiteCommand($"SELECT * FROM {table} WHERE ID={id}", DataConnection);
            Adapters[table].Fill(result);
            return result.Rows.Count > 0 ? result.Rows[0] : null;
        }

        public static int Save(string table, params (string key, object value)[] columns)
        {
            if (columns.Any(c => c.key.Equals("Removed") && (bool)c.value == true))
                return -1;

            var adapter = Adapters[table];
            adapter.SelectCommand = new SQLiteCommand($"SELECT * FROM {table}", DataConnection);
            var data = new DataTable(table);
            adapter.Fill(data);
            data.PrimaryKey = new[] { data.Columns["ID"] };


            DataRow row;
            int id;
            bool isLocal = true, addMode = true;
            if (columns.Any(c => c.key.Equals("IsLocal")) &&
                !(isLocal = bool.Parse(columns.First(c => c.key.Equals("IsLocal")).value.ToString())) &&
                (id = int.Parse(columns.First(c => c.key.Equals("ID")).value.ToString())) > 0 &&
                data.Rows.Contains(id))
            {
                row = data.Rows.Find(id);
                addMode = false;
            }
            else
            {
                row = data.NewRow();
                row["ID"] = id = GetFreeID(table);
            }

            row["IsLocal"] = false;
            row["Modified"] = false;

            foreach (var current in columns)
                if ((current.key != "ID" || isLocal) && current.key != "IsLocal" && current.key != "Modified")
                    row[current.key] = current.value;

            Adapters[table].SelectCommand = new SQLiteCommand($"SELECT * FROM {table} WHERE ID={id}", DataConnection);

            if (addMode)
                data.Rows.Add(row);

            adapter.InsertCommand = new SQLiteCommandBuilder(adapter).GetInsertCommand(true);
            adapter.UpdateCommand = new SQLiteCommandBuilder(adapter).GetUpdateCommand(true);

            adapter.Update(data);

            return id;
        }

        public static int Remove(string table, int id)
        {
            return new SQLiteCommand($"DELETE FROM {table} WHERE ID={id}", DataConnection).ExecuteNonQuery();
        }

        static int GetFreeID(string table)
        {
            Adapters[table].SelectCommand = new SQLiteCommand($"SELECT Max(Count(*),Max(ID)) FROM {table}", DataConnection);
            var data = new DataTable();
            return Adapters[table].Fill(data) == 0 ? 1 : string.IsNullOrWhiteSpace(data.Rows[0][0].ToString()) ? 1 : Convert.ToInt32(data.Rows[0][0]) + 1;
        }
    }
}
