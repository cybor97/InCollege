using InCollege.Client.UI;
using System.Windows;

namespace InCollege.Client
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainWindow mainWindow = new MainWindow();

            MainWindow = mainWindow;

            var loginWindow = new AuthorizationWindow();
            loginWindow.ShowDialog();
            if (loginWindow.DialogResult.HasValue && loginWindow.DialogResult.Value)
                mainWindow.ShowDialog();
            else Shutdown();
        }
    }
}
