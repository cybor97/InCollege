using System.Data.Entity;
namespace InCollege.Core.Data.Base
{
    public abstract class BaseDBContext : DbContext
    {
        protected BaseDBContext(string connectionString) : base(connectionString) { }
        public abstract void Init(string connectionString);
        protected BaseDBContext instance;
        public BaseDBContext Instance
        {
            get
            {
                return instance;
            }
        }
    }

    public class DBContextSQLite : BaseDBContext
    {
        private DBContextSQLite(string connectionString) : base(connectionString) { }
        public override void Init(string connectionString)
        {
            instance = new DBContextSQLite(connectionString);
        }
    }

    public class DBContextMySQL : BaseDBContext
    {
        private DBContextMySQL(string connectionString) : base(connectionString) { }
        public override void Init(string connectionString)
        {
            instance = new DBContextMySQL(connectionString);
        }
    }
}
