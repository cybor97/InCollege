using InCollege.Core.Data.Base;

namespace InCollege.Server
{
    class Program
    {
        public static void Main()
        {
            DBHolderSQL.Init(CommonVariables.DBLocation);
            InCollegeServer.Start();
            while (true) System.Threading.Thread.Sleep(100);
        }
    }
}
