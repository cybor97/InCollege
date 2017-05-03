using InCollege.Client.UI.AuthorizationUI;
using InCollege.Client.UI.MainUI;
using InCollege.Core.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace InCollege.Client
{
    public partial class App : Application
    {
        static string _token = null;
        public static string Token
        {
            get
            {
                if (_token == null && File.Exists(CommonVariables.TokenFileName))
                    _token = File.ReadAllText(CommonVariables.TokenFileName);
                return _token;
            }
            set
            {
                if (!Directory.Exists(CommonVariables.DataDirectory))
                    Directory.CreateDirectory(CommonVariables.DataDirectory);
                _token = value;
                if (value != null)
                    File.WriteAllText(CommonVariables.TokenFileName, _token);
                else
                    File.Delete(CommonVariables.TokenFileName);
            }
        }
        public static Account Account { get; set; }

        public App()
        {
            InitializeComponent();

            MainWindow = new MainWindow();

            var loginWindow = new AuthorizationWindow();
            if (Token != null && ValidateToken() ||
                loginWindow.ShowDialog().HasValue && loginWindow.DialogResult.Value)
                MainWindow.ShowDialog();
            else Shutdown();
        }

        static bool ValidateToken()
        {
            try
            {
                var task = new HttpClient().PostAsync(ClientConfiguration.AuthHandlerPath, new StringContent($"Action=ValidateToken&token={Token}"));
                task.Wait();
                return task.Result.StatusCode == HttpStatusCode.OK;
            }
            catch (AggregateException e)
            {
                //UNDONE:Remove this debug staff!
                MessageBox.Show(e.ToString());
                return true;
            }
        }
    }
}
