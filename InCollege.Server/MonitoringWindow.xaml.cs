using System.Windows;

namespace InCollege.Server
{
    public partial class MainWindow : Window
    {
        private static MainWindow CurrentInstance;
        public MainWindow()
        {
            InitializeComponent();
            CurrentInstance = this;
            InCollegeServer.Start();
        }

        public static void Log(string message)
        {
            CurrentInstance.ConsoleContent.AppendText(message + "\n");
        }
    }
}
