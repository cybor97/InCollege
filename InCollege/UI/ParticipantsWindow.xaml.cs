using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InCollege.Client.UI
{
    public partial class ParticipantsWindow : Window
    {
        bool AddMode { get; set; }

        public ParticipantsWindow(AccountType accountType)
        {
            InitializeComponent();
            AccountTypesTabs.SelectedIndex = (int)accountType;
        }

        async Task UpdateDisplayData()
        {
            try
            {
                AccountsLV.Items.Clear();
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={nameof(Account)}&" +
                      $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<IEnumerable<Account>>(await response.Content.ReadAsStringAsync());
                    //Yeah, 2 times, to prevent multiplying
                    AccountsLV.Items.Clear();
                    foreach (var current in result)
                        if ((!current.Approved && AccountTypesTabs.SelectedIndex == 0) || (int)current.AccountType == AccountTypesTabs.SelectedIndex)
                            AccountsLV.Items.Add(current);
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                    if (response.StatusCode == HttpStatusCode.Forbidden) Close();
                }
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
        }

        async void ParticipantsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        async void ProfileDialog_OnSave(object sender, RoutedEventArgs e)
        {
            ProfileDialog.Account.IsLocal = AddMode;
            if (await SaveAccount(ProfileDialog.Account))
                await UpdateDisplayData();
            ProfileDialog.IsOpen = false;
        }

        private void ProfileDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddMode = true;
            ProfileDialog.Account = new Account { AccountType = (AccountType)AccountTypesTabs.SelectedIndex };
            ProfileDialog.IsOpen = true;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsLV.SelectedIndex != -1 && AccountsLV.SelectedItems.Count == 1)
            {
                ProfileDialog.IsOpen = true;
                ProfileDialog.Account = (Account)AccountsLV.SelectedItem;
                AddMode = false;
            }
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            bool updateRequired = false;
            if (AccountsLV.SelectedItems?.Count > 0 && MessageBox.Show("Вы уверены, что хотите удалить выбранных пользователей? Авторизация для них будет невозможна!", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                try
                {
                    foreach (DBRecord current in AccountsLV.SelectedItems)
                    {
                        HttpResponseMessage response;
                        if ((response = (await new HttpClient()
                              .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                              new StringContent(
                              $"Action=Remove&" +
                              $"token={App.Token}&" +
                              $"table={nameof(Account)}&" +
                              $"id={current.ID}")))).StatusCode == HttpStatusCode.OK)
                            updateRequired = updateRequired || response.StatusCode == HttpStatusCode.OK;
                        else
                        {
                            MessageBox.Show(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
                catch (HttpRequestException exc)
                {
                    MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
                }

            if (updateRequired)
                await UpdateDisplayData();
        }

        async void ApprovedCB_CheckChanged(object sender, RoutedEventArgs e)
        {
            var item = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);

            bool updateRequired = false;
            if (item != null)
            {
                var account = (Account)item.DataContext;
                if (account.ID != App.Account.ID || MessageBox.Show("Внимание! Это действие приведет к деактивации вашего аккаунта, дальнейшая работа будет невозможна. Вы уверены?",
                    "Деактивация аккаунта",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    updateRequired = updateRequired || await SaveAccount(account);
                else updateRequired = true;
            }
            if (updateRequired)
                await UpdateDisplayData();
        }

        public static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T objTest)
                    return objTest;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        async void AccountTypesTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateDisplayData();
        }

        async Task<bool> SaveAccount(Account account)
        {
            try
            {
                HttpResponseMessage response;
                if (AddMode)
                    ProfileDialog.Account.IsLocal = true;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=Save&" +
                      $"token={App.Token}&" +
                      account.POSTSerialized)))).StatusCode == HttpStatusCode.OK)
                    return true;
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
            return false;
        }
    }
}
