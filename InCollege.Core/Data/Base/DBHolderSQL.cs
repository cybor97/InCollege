using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
//TODO:Fix concurency
namespace InCollege.Core.Data.Base
{
    /// <summary>
    /// DO NOT USE ON CLIENT SIDE!!!11
    /// </summary>
    public static class DBHolderSQL
    {
        static SQLiteConnection DataConnection;
        public static void Init(string filename)
        {
            //Create required tables and... at all, prepare db.
            //I hope this isn't shit-code-design.
            DBHolderORM.Init(filename);

            DataConnection = new SQLiteConnection($"Data Source={filename}; Version=3;");
            DataConnection.Open();
        }

        public static DataTable GetRange(string table, string column, int skip, int count, bool fixedString, bool justCount, bool reverse, params (string name, object value)[] whereParams)
        {
            column = string.IsNullOrWhiteSpace(column) ? "*" : $"[{column}]";

            string whereString = string.Join(" AND ", whereParams
                .Where(c => !string.IsNullOrWhiteSpace(c.name) && !string.IsNullOrWhiteSpace(c.value.ToString()))
                .Select(c => !(c.value is string) || fixedString ?
                        $"{c.name} LIKE @{c.name}" :
                        $"instr({c.name}, @{c.name}) > 0"));

            var adapter = new SQLiteDataAdapter($"SELECT " +
                                  (justCount ? "count()" : $"{column} ") +
                                  $"FROM [{table}] " +
                                  (string.IsNullOrWhiteSpace(whereString) ? "" : $"WHERE {whereString} ") +
                                  (reverse ? column == "*" ? "ORDER BY ID DESC " : $"ORDER BY {column} DESC " : "") +
                                  $"LIMIT {skip}, {(count == -1 ? DBHolderORM.DEFAULT_LIMIT : count)} ",
                    DataConnection);
            foreach (var current in whereParams)
                adapter.SelectCommand.Parameters.AddWithValue($"@{current.name}", current.value);

            var result = new DataTable(table);
            adapter.Fill(result);
            return result;
        }

        public static DataRow GetByID(string table, int id)
        {
            DataTable result = new DataTable();
            var adapter = new SQLiteDataAdapter($"SELECT * FROM [{table}] WHERE ID={id}", DataConnection);
            adapter.Fill(result);
            return result.Rows.Count > 0 ? result.Rows[0] : null;
        }

        public static long Save(string table, params (string key, object value)[] columns)
        {
            bool isLocal = false;
            long id = -1;

            for (int i = 0; i < columns.Length; i++)
            {
                var c = columns[i];
                if (c.key == "Removed" && c.value.ToString() == "1")
                    return -1;
                if (c.key == "ID")
                    id = long.Parse(c.value.ToString());
                else if ((c.key == "IsLocal" || c.key == "Modified" || c.key == "Removed") && !(c.value is bool))
                {
                    columns[i] = (c.key, (int.Parse(c.value.ToString()) == 1));
                    if (c.key == "IsLocal") isLocal = (bool)columns[i].value;
                }
            }

            lock (DataConnection)
                using (var transaction = DataConnection.BeginTransaction(IsolationLevel.Serializable))
                using (var adapter = new SQLiteDataAdapter($"SELECT * FROM [{table}] LIMIT 0, 1", DataConnection))
                {
                    var data = new DataTable(table);
                    adapter.Fill(data);
                    data.PrimaryKey = new[] { data.Columns["ID"] };


                    DataRow row;
                    bool addMode = true;
                    if (!isLocal && (long)new SQLiteCommand($"SELECT Count() FROM {table} WHERE ID LIKE '{id}'", DataConnection).ExecuteScalar() > 0)
                    {
                        adapter.SelectCommand = new SQLiteCommand($"SELECT * FROM [{table}] WHERE ID LIKE '{id}'", DataConnection) { Transaction = transaction };
                        data.Clear();
                        adapter.Fill(data);
                        row = data.Rows[0];
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
                            if (current.value == null || !current.value.ToString().StartsWith("raw_data"))
                                row[current.key] = current.value;
                            else row[current.key] = Convert.FromBase64String(current.value.ToString().Split(new[] { "raw_data" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                    adapter.SelectCommand = new SQLiteCommand($"SELECT * FROM [{table}] WHERE ID={id}", DataConnection) { Transaction = transaction };

                    if (addMode)
                        data.Rows.Add(row);

                    adapter.InsertCommand = new SQLiteCommandBuilder(adapter).GetInsertCommand(true);
                    adapter.InsertCommand.Transaction = transaction;
                    adapter.UpdateCommand = new SQLiteCommandBuilder(adapter).GetUpdateCommand(true);
                    adapter.UpdateCommand.Transaction = transaction;

                    adapter.Update(data);
                    transaction.Commit();
                }
            return id;
        }

        public static int Remove(string table, int id)
        {
            return new SQLiteCommand($"DELETE FROM [{table}] WHERE ID={id}", DataConnection).ExecuteNonQuery();
        }

        static int GetFreeID(string table)
        {
            var data = new DataTable();
            new SQLiteDataAdapter($"SELECT * FROM sqlite_sequence WHERE name LIKE '{table}';", DataConnection).Fill(data);
            return data.Rows.Count == 0 ? 0 : Convert.ToInt32(data.Rows[0]["seq"]) + 1;
        }
    }
}
