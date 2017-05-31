using InCollege.Client.UI.AccountsUI;
using InCollege.Client.UI.MainUI;
using InCollege.Core.Data;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Resources;
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
                return WebUtility.UrlEncode(_token);
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

            ResourceSet resourceSet = InCollege.Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            if (!Directory.Exists(CommonVariables.TemplatesDirectory))
            {
                Directory.CreateDirectory(CommonVariables.TemplatesDirectory);
                foreach (DictionaryEntry entry in resourceSet)
                    if (entry.Key.ToString().EndsWith("_TEMPLATE"))
                        File.WriteAllBytes(Path.Combine(CommonVariables.TemplatesDirectory, $"{entry.Key.ToString().Split(new[] { "_TEMPLATE" }, StringSplitOptions.RemoveEmptyEntries)[0]}.docx"), (byte[])entry.Value);
            }

            var loginWindow = new AuthorizationWindow();
            if (Token != null && ValidateToken() ||
                loginWindow.ShowDialog().HasValue && loginWindow.DialogResult.Value)
                MainWindow.ShowDialog();
            else Shutdown();

            try
            {
                NetworkUtils.Disconnect().Wait();
            }
            catch (Exception e) when (e is AggregateException || e is HttpRequestException || e is WebException)
            {

            }
        }

        static bool ValidateToken()
        {
            try
            {
                var task = new HttpClient().PostAsync(ClientConfiguration.Instance.AuthHandlerPath, new StringContent($"Action=ValidateToken&token={Token}"));
                task.Wait();
                return task.Result.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e) when (e is AggregateException || e is HttpRequestException || e is WebException)
            {
                MessageBox.Show($"Ошибка подключения к серверу!\n\nТехническая информация:\n{e}");
                return false;
            }
        }
    }
}
