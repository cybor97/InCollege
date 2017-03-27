using System.Data.Common;
using System.Data.Entity;
namespace InCollege.Core.Data.Base
{
    public class DBHolder : DbContext
    {
        public DBHolder(string connectionString) : base(connectionString) { }

        public DBHolder(DbConnection connection) : base(connection, true) { }

        public DbSet<Account> Accounts { get; set; }
    }
}
