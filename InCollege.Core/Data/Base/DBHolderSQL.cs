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

            DataConnection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;", filename));
            DataConnection.Open();
            Adapters = new Dictionary<string, SQLiteDataAdapter>();

            foreach (DataRow current in DataConnection.GetSchema("Tables").Rows)
                Adapters.Add(current[2].ToString(), new SQLiteDataAdapter("SELECT * FROM {0} LIMIT {1}, {2}", DataConnection));
        }

        public static DataTable GetRange(string table, string column, int skip, int count, bool fixedString, params (string, object)[] whereParams)
        {
            if (count == -1)
                count = DBHolderORM.DEFAULT_LIMIT;
            if (table == null)
                table = "master_table";
            if (string.IsNullOrWhiteSpace(column))
                column = "*";
            DataTable result = new DataTable(table);
            string whereString = null;
            if (whereParams != null)
            {
                whereString = "";
                for (int i = 0; i < whereParams.Length; i++)
                {
                    whereString += string.Format(whereParams[i].Item2 is string ? fixedString ? "{0} LIKE '{1}'" : "instr({0}, '{1}') > 0" :
                        "{0} LIKE {1}", whereParams[i].Item1, whereParams[i].Item2);
                    if (i < whereParams.Length - 1)
                        whereString += " AND ";
                }
            }
            Adapters[table].SelectCommand =
                new SQLiteCommand(
                    string.Format("SELECT {0} FROM {1} " +
                                  (string.IsNullOrWhiteSpace(whereString) ? "" : "WHERE {4} ") +
                                  "LIMIT {2}, {3} ",
                                  column, table, skip, count, whereString),
                    DataConnection);
            Adapters[table].Fill(result);
            return result;
        }

        public static DataRow GetByID(string table, int id)
        {
            DataTable result = new DataTable();
            Adapters[table].SelectCommand = new SQLiteCommand(string.Format("SELECT * FROM {0} WHERE ID={1}", table, id), DataConnection);
            Adapters[table].Fill(result);
            return result.Rows.Count > 0 ? result.Rows[0] : null;
        }

        public static int Save(string table, params (string, object)[] columns)
        {
            var adapter = Adapters[table];
            adapter.SelectCommand = new SQLiteCommand(string.Format("SELECT * FROM {0}", table), DataConnection);
            var data = new DataTable(table);
            adapter.Fill(data);
            data.PrimaryKey = new[] { data.Columns["ID"] };


            DataRow row;
            int id;
            bool isLocal = true, addMode = true;
            if (columns.Any(c => c.Item1.Equals("IsLocal")) &&
                !(isLocal = bool.Parse(columns.First(c => c.Item1.Equals("IsLocal")).Item2.ToString())) &&
                (id = int.Parse(columns.First(c => c.Item1.Equals("ID")).Item2.ToString())) > 0 &&
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
                if ((current.Item1 != "ID" || isLocal) && current.Item1 != "IsLocal")
                    row[current.Item1] = current.Item2;

            Adapters[table].SelectCommand = new SQLiteCommand(string.Format("SELECT * FROM {0}", table), DataConnection);

            if (addMode)
                data.Rows.Add(row);

            adapter.InsertCommand = new SQLiteCommandBuilder(adapter).GetInsertCommand(true);
            adapter.UpdateCommand = new SQLiteCommandBuilder(adapter).GetUpdateCommand(true);

            adapter.Update(data);

            return id;
        }

        public static int Remove(string table, int id)
        {
            return new SQLiteCommand(string.Format("DELETE FROM {0} WHERE ID={1}",table,id), DataConnection).ExecuteNonQuery();
        }

        static int GetFreeID(string table)
        {
            Adapters[table].SelectCommand = new SQLiteCommand(string.Format("SELECT ID FROM {0}", table), DataConnection);
            DataTable data = new DataTable();
            Adapters[table].Fill(data);
            int result = 0;
            foreach (DataRow current in data.Rows)
                if (int.Parse(current["ID"].ToString()) > result) result = int.Parse(current["ID"].ToString());
            return result + 1;
        }
    }
}
