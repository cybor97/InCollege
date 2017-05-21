using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using InCollege.Client.UI.Util;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InCollege.Client.UI.AccountsUI
{
    public partial class AccountsWindow : Window, IUpdatable
    {
        bool AddMode { get; set; }

        public AccountsWindow(AccountType accountType)
        {
            InitializeComponent();
            AccountTypesTabs.SelectedIndex = (int)accountType;
        }

        public async Task UpdateData()
        {
            AccountsLV.ItemsSource = (await NetworkUtils.RequestData<Account>(this))
                ?.Where(c => (!c.Approved && AccountTypesTabs.SelectedIndex == 0) || (int)c.AccountType == AccountTypesTabs.SelectedIndex);
        }

        async void ParticipantsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        async void ProfileDialog_OnSave(object sender, RoutedEventArgs e)
        {
            ProfileDialog.Account.IsLocal = AddMode;
            await SaveAccount(ProfileDialog.Account);
            ProfileDialog.IsOpen = false;
        }

        void ProfileDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddMode = true;
            ProfileDialog.Account = new Account { AccountType = (AccountType)AccountTypesTabs.SelectedIndex };
            ProfileDialog.IsOpen = true;
        }

        void EditItem_Click(object sender, RoutedEventArgs e)
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
            if (AccountsLV.SelectedItems?.Count > 0)
                try
                {
                    foreach (DBRecord current in AccountsLV.SelectedItems)
                        updateRequired = updateRequired ||
                             (int.TryParse(await NetworkUtils.ExecuteDataAction<Account>(null, current, DataAction.Remove), out int newID) &&
                             newID > -1);
                }
                catch (HttpRequestException exc)
                {
                    MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
                }

            if (updateRequired)
                await UpdateData();
        }

        async void ApprovedCB_CheckChanged(object sender, RoutedEventArgs e)
        {
            var item = UIUtils.FindAncestorOrSelf<ListViewItem>(sender as CheckBox);

            if (item != null)
            {
                AddMode = false;
                var account = (Account)item.DataContext;
                if (account.ID != App.Account.ID || MessageBox.Show("Внимание! Это действие приведет к деактивации вашего аккаунта, дальнейшая работа будет невозможна. Вы уверены?",
                    "Деактивация аккаунта",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    await SaveAccount(account);
                else if (account.ID == App.Account.ID)
                    await UpdateData();
            }
        }

        async void AccountTypesTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateData();
        }

        async Task SaveAccount(Account account)
        {
            if (AddMode)
            {
                account.IsLocal = true;
                account.ID = -1;
            }

            await NetworkUtils.ExecuteDataAction<Account>(this, account, DataAction.Save);
        }

        private void ProfileDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (ProfileDialog.IsOpen)
                if (e.Key == Key.Enter)
                {
                    ProfileDialog.FullNameTB.Focus();
                    ProfileDialog.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    ProfileDialog_OnSave(null, null);
                }
                else if (e.Key == Key.Escape)
                    ProfileDialog_OnCancel(null, null);
        }

        private async void AccountsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                case Key.OemPlus:
                    AddButton_Click(null, null);
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                case Key.Delete:
                    RemoveItem_Click(null, null);
                    break;
                case Key.F5:
                    await UpdateData();
                    break;
            }
        }
    }
}
