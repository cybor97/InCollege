using InCollege.Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InCollege.Client.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        private async Task UpdateDisplayData()
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
                    var result = JsonConvert.DeserializeObject<IList<Account>>(await response.Content.ReadAsStringAsync())[0];
                    CurrentAccountItem.Header = FullNameTB.Text = result.FullName;
                    UserNameTB.Text = result.UserName;
                    BirthDateTB.SelectedDate = result.BirthDate;
                    AccountTypeCB.SelectedIndex = (byte)result.AccountType;
                    if (!result.Approved)
                    {
                        ProfileDialog.IsOpen = true;
                        MessageBox.Show("Извините, ваша должность не подтверждена. Обратитесь к администратору.");
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

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DictionariesItem_Click(object sender, RoutedEventArgs e)
        {
            new DictionariesWindow().ShowDialog();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void ParticipantsItem_Click(object sender, RoutedEventArgs e)
        {
            new ParticipantsWindow().ShowDialog();
        }

        private void AccountExitItem_Click(object sender, RoutedEventArgs e)
        {
            AccountExit();
        }

        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                await UpdateDisplayData();
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented :(");
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented :(");
        }

        private void ProfileDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ProfileDialog.IsOpen = false;
        }

        private void CurrentAccountItem_Click(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = true;
        }

        void AccountExit()
        {
            App.Token = null;
            Process.Start(Assembly.GetExecutingAssembly().Location);
            Process.GetCurrentProcess().Kill();
        }

        private void ProfileSaveButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }

        private void ProfileCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }
    }
}
