using InCollege.Core.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace InCollege.Client.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task UpdateDisplayData()
        {
            try
            {
                StatementsLV.Items.Clear();
                HttpResponseMessage response;
                //TODO:
                //if(teacher) request<statement>(teacher.subjects);
                //else if (departmentHead) request<statement>(departmentHead.groups);
                //else if (admin) request<statement>(all);
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={nameof(Statement)}&" +
                      $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<IEnumerable<Statement>>(await response.Content.ReadAsStringAsync());
                    //Yeah, 2 times, to prevent multiplying
                    StatementsLV.Items.Clear();
                    foreach (var current in result)
                        StatementsLV.Items.Add(current);
                }
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DictionariesItem_Click(object sender, RoutedEventArgs e)
        {
            new DictionariesWindow().ShowDialog();
        }

        private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void ParticipantsItem_Click(object sender, RoutedEventArgs e)
        {
            new ParticipantsWindow().ShowDialog();
        }

        private void AccountExitItem_Click(object sender, RoutedEventArgs e)
        {
            App.Token = null;
            Process.Start(Assembly.GetExecutingAssembly().Location);
            Process.GetCurrentProcess().Kill();
        }
    }
}
