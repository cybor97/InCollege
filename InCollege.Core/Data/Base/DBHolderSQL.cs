using System;
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
        static string ConnectionString { get; set; }

        public static void Init(string filename)
        {
            //Create required tables and... at all, prepare db.
            //I hope this isn't shit-code-design.
            DBHolderORM.Init(filename);

            ConnectionString =
                new SQLiteConnectionStringBuilder(string.Empty)
                {
                    DataSource = filename,
                    Version = 3,
                    JournalMode = SQLiteJournalModeEnum.Memory,
                }.ConnectionString;
        }

        public static DataTable GetFriends(int accountID)
        {
            var table = new DataTable();
            using (var connection = new SQLiteConnection(ConnectionString).OpenAndReturn())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Unspecified))
            using (var adapter = new SQLiteDataAdapter { AcceptChangesDuringFill = false })
            {
                adapter.SelectCommand = new SQLiteCommand("SELECT DISTINCT Account.FullName, Account.ID " +
                    "FROM Message " +
                    "LEFT JOIN Account ON Account.ID = Message.FromID OR Account.ID = Message.ToID " +
                    "WHERE (Message.FromID=@accountID OR Message.ToID=@accountID) AND Account.ID<>@accountID;", connection)
                { Transaction = transaction };
                adapter.SelectCommand.Parameters.AddWithValue("@accountID", accountID);
                adapter.Fill(table);
            }
            return table;
        }

        public static DataTable GetRange(string table, string column, int skip, int count, bool fixedString, bool justCount, bool reverse, bool orAll, params (string name, object value)[] whereParams)
        {
            var result = new DataTable(table);
            using (var connection = new SQLiteConnection(ConnectionString).OpenAndReturn())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Unspecified))
            using (var adapter = new SQLiteDataAdapter { AcceptChangesDuringFill = false })
                try
                {
                    column = string.IsNullOrWhiteSpace(column) ? "*" : $"[{column}]";

                    string whereString = string.Join(

                        orAll ? " OR " : " AND ",

                        whereParams
                        .Where(c => !string.IsNullOrWhiteSpace(c.name) && !string.IsNullOrWhiteSpace(c.value?.ToString()))
                        .Select(c => !(c.value is string) || fixedString ?
                                $"{c.name} LIKE @{c.name}" :
                                $"instr({c.name}, @{c.name}) > 0"));

                    adapter.SelectCommand = new SQLiteCommand($"SELECT " +
                                          (justCount ? "count()" : $"{column} ") +
                                          $"FROM [{table}] " +
                                          (string.IsNullOrWhiteSpace(whereString) ? "" : $"WHERE {whereString} ") +
                                          (reverse ? column == "*" ? "ORDER BY ID DESC " : $"ORDER BY {column} DESC " : "") +
                                          $"LIMIT {skip}, {(count == -1 ? DBHolderORM.DEFAULT_LIMIT : count)} ",
                            connection)
                    { Transaction = transaction };
                    foreach (var current in whereParams)
                        adapter.SelectCommand.Parameters.AddWithValue($"@{current.name}", current.value);

                    adapter.Fill(result);
                }
                catch (ObjectDisposedException)
                {
                    lock (ConnectionString)
                        result = GetRange(table, column, skip, count, fixedString, justCount, reverse, orAll, whereParams);
                    return result;
                }
            return result;
        }

        public static DataRow GetByID(string table, int id)
        {
            DataTable result = new DataTable();
            using (var connection = new SQLiteConnection(ConnectionString).OpenAndReturn())
            {
                var adapter = new SQLiteDataAdapter($"SELECT * FROM [{table}] WHERE ID={id}", connection);
                adapter.Fill(result);
            }
            return result.Rows.Count > 0 ? result.Rows[0] : null;
        }


        public static void Log(string message, string description, int accountID = -1)
        {
            Save(nameof(Log), new Log
            {
                AccountID = accountID,
                Message = message,
                Description = description,
                LogDate = DateTime.Now,
            }.Columns.ToArray());
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

            using (var connection = new SQLiteConnection(ConnectionString).OpenAndReturn())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Unspecified))
            using (var adapter = new SQLiteDataAdapter($"SELECT * FROM [{table}] LIMIT 0, 1", connection))
            {
                var data = new DataTable(table);
                adapter.Fill(data);
                data.PrimaryKey = new[] { data.Columns["ID"] };


                DataRow row;
                bool addMode = true;
                if (!isLocal && (long)new SQLiteCommand($"SELECT Count() FROM [{table}] WHERE ID LIKE '{id}'") { Transaction = transaction }.ExecuteScalar() > 0)
                {
                    adapter.SelectCommand = new SQLiteCommand($"SELECT * FROM [{table}] WHERE ID LIKE '{id}'") { Transaction = transaction };
                    data.Clear();
                    adapter.Fill(data);
                    row = data.Rows[0];
                    addMode = false;
                }
                else
                {
                    row = data.NewRow();
                    row["ID"] = id = GetFreeID(connection, table);
                }

                row["IsLocal"] = false;
                row["Modified"] = false;

                foreach (var current in columns)
                    if ((current.key != "ID") && current.key != "IsLocal" && current.key != "Modified")
                        if (current.value == null || !current.value.ToString().StartsWith("raw_data"))
                            row[current.key] = current.value;
                        else row[current.key] = Convert.FromBase64String(current.value.ToString().Split(new[] { "raw_data" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                adapter.SelectCommand = new SQLiteCommand($"SELECT * FROM [{table}] WHERE ID={id}") { Transaction = transaction };

                if (addMode)
                    data.Rows.Add(row);

                if (addMode)
                {
                    var command = new SQLiteCommand($"INSERT INTO [{table}] " +
                        $"({string.Join(", ", columns.Select(c => c.key))}) " +
                        $"VALUES ({string.Join(", ", columns.Select(c => $"@{c.key}"))});")
                    {
                        Transaction = transaction
                    };
                    command.Parameters.AddRange(columns.Select(c => new SQLiteParameter(c.key, row[c.key])).ToArray());

                    command.ExecuteNonQuery();
                }
                else
                {
                    var command = new SQLiteCommand($"UPDATE [{table}] " +
                        $"SET {string.Join(", ", columns.Select(c => $"{c.key} = @{c.key} "))}" +
                        $"WHERE ID = @ID")
                    { Transaction = transaction };
                    command.Parameters.AddRange(columns.Select(c => new SQLiteParameter(c.key, row[c.key])).ToArray());

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            return id;
        }

        public static int Remove(string table, int id)
        {
            int result = -1;
            using (var connection = new SQLiteConnection(ConnectionString).OpenAndReturn())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                result = new SQLiteCommand($"DELETE FROM [{table}] WHERE ID={id}") { Transaction = transaction }.ExecuteNonQuery();
                transaction.Commit();
            }
            return result;
        }

        public static int RemoveWhere(string table, params (string name, object value)[] whereParams)
        {
            int result = -1;
            using (var connection = new SQLiteConnection(ConnectionString).OpenAndReturn())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                string whereString = whereParams.Length == 0 ? "" : "WHERE " +
                      string.Join(" AND ", whereParams.Where(c => !string.IsNullOrWhiteSpace(c.name) && !string.IsNullOrWhiteSpace(c.value.ToString()))
                      .Select(c => $"{c.name} LIKE @{c.name}"));

                var command = new SQLiteCommand($"DELETE FROM [{table}] {whereString}") { Transaction = transaction };
                command.Parameters.AddRange(whereParams.Select(c => new SQLiteParameter(c.name, c.value)).ToArray());
                result = command.ExecuteNonQuery();
                transaction.Commit();
            }
            return result;
        }

        static int GetFreeID(SQLiteConnection connection, string table)
        {
            var data = new DataTable();
            new SQLiteDataAdapter($"SELECT * FROM sqlite_sequence WHERE name LIKE '{table}';", connection).Fill(data);
            return data.Rows.Count == 0 ? 0 : Convert.ToInt32(data.Rows[0]["seq"]) + 1;
        }
    }
}
