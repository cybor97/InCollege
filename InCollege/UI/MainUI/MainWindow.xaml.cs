using InCollege.Client.UI.AccountsUI;
using InCollege.Client.UI.DictionariesUI;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using InCollege.UI;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InCollege.Client.UI.MainUI
{
    public partial class MainWindow : Window, IUpdatable
    {
        public MainWindow()
        {
            InitializeComponent();

            EditStatementDialog.OnSave += EditStatementDialog_OnSave;
            EditStatementDialog.OnCancel += EditStatementDialog_OnCancel;
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            CurrentAccountItem.Header = (ProfileDialog.Account = App.Account = await NetworkUtils.WhoAmI())?.FullName;

            if (App.Account == null || !App.Account.Approved)
            {
                if (App.Account != null)
                    MessageBox.Show("Извините, ваша должность не подтверждена. Обратитесь к администратору.");
                AccountExit();
            }


            //TODO:
            //if(teacher) request<statement>(teacher.subjects);
            //else if (departmentHead) request<statement>(departmentHead.groups);
            //else if (admin) request<statement>(all);
            StatementsLV.ItemsSource = await NetworkUtils.RequestData<Statement>(this);
        }

        void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        async void DictionariesItem_Click(object sender, RoutedEventArgs e)
        {
            int itemIndex = ((MenuItem)(((MenuItem)sender).Parent)).Items.IndexOf(sender);
            if (itemIndex == 0) new DictionariesWindow().ShowDialog();
            else new StudyObjectsWindow(itemIndex - 1).ShowDialog();
            await UpdateData();
        }

        void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        async void ParticipantsItem_Click(object sender, RoutedEventArgs e)
        {
            new AccountsWindow((AccountType)((MenuItem)(((MenuItem)sender).Parent)).Items.IndexOf(sender)).ShowDialog();
            await UpdateData();
        }

        void AccountExitItem_Click(object sender, RoutedEventArgs e)
        {
            AccountExit();
        }

        async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                await UpdateData();
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
            if (ProfileDialog.Account != null)
                await NetworkUtils.ExecuteDataAction<Account>(this, ProfileDialog.Account, DataAction.Save);

            ProfileDialog.IsOpen = false;
        }

        private void ProfileDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }
    }
}
