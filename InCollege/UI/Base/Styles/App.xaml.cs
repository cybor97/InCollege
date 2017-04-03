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

            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            if (loginWindow.DialogResult.HasValue && loginWindow.DialogResult.Value)
                mainWindow.ShowDialog();
        }
    }
}
