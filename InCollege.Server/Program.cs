using InCollege.Core.Data.Base;
using System;
using System.IO;

namespace InCollege.Server
{
    class Program
    {
        public static void Main()
        {
            DBHolderSQL.Init(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.db"));
            InCollegeServer.Start();
            while (true) System.Threading.Thread.Sleep(100);
        }
    }
}
