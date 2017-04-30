using InCollege.Client.UI.DictionariesUI;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InCollege.Client.UI.MainUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            EditStatementDialog.OnSave += EditStatementDialog_OnSave;
            EditStatementDialog.OnCancel += EditStatementDialog_OnCancel;
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        async Task UpdateDisplayData()
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Auth",
                new StringContent(
                $"Action=WhoAmI&" +
                $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                {
                    App.Account = JsonConvert.DeserializeObject<IList<Account>>(await response.Content.ReadAsStringAsync())[0];
                    CurrentAccountItem.Header = App.Account.FullName;
                    ProfileDialog.Account = App.Account;
                    if (!App.Account.Approved)
                    {
                        MessageBox.Show("Извините, ваша должность не подтверждена. Обратитесь к администратору.");
                        AccountExit();
                    }
                }
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());


                StatementsLV.Items.Clear();
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

        void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        async void DictionariesItem_Click(object sender, RoutedEventArgs e)
        {
            new DictionariesWindow().ShowDialog();
            await UpdateDisplayData();
        }

        void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        async void ParticipantsItem_Click(object sender, RoutedEventArgs e)
        {
            new ParticipantsWindow((AccountType)((MenuItem)(((MenuItem)sender).Parent)).Items.IndexOf(sender)).ShowDialog();
            await UpdateDisplayData();
        }

        void AccountExitItem_Click(object sender, RoutedEventArgs e)
        {
            AccountExit();
        }

        async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                await UpdateDisplayData();
        }

        void EditItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented :(");
        }

        void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented :(");
        }

        void ProfileDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ProfileDialog.IsOpen = false;
        }

        void CurrentAccountItem_Click(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = true;
        }

        void AccountExit()
        {
            App.Token = null;
            Process.Start(Assembly.GetExecutingAssembly().Location);
            Process.GetCurrentProcess().Kill();
        }


        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            EditStatementDialog.IsOpen = true;
        }

        void EditStatementDialog_OnSave(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented :(");
            EditStatementDialog.IsOpen = false;
        }

        void EditStatementDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            EditStatementDialog.IsOpen = false;
        }

        private async void ProfileDialog_OnSave(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=Save&" +
                      $"token={App.Token}&" +
                      ProfileDialog.Account.POSTSerialized)))).StatusCode == HttpStatusCode.OK)
                    await UpdateDisplayData();
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }

            ProfileDialog.IsOpen = false;
        }

        private void ProfileDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }
    }
}
