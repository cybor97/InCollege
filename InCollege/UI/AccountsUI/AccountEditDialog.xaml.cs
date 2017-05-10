using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace InCollege.Client.UI.AccountsUI
{
    public partial class AccountEditDialog : DialogHost
    {
        public static DependencyProperty ShowAccountChangeAlertProperty =
            DependencyProperty.Register("ShowAccountChangeAlert", typeof(Account), typeof(AccountEditDialog));
        public bool ShowAccountChangeAlert { get; set; }

        public static readonly RoutedEvent OnSaveEvent = EventManager.RegisterRoutedEvent("OnSave", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AccountEditDialog));
        public event RoutedEventHandler OnSave
        {
            add => AddHandler(OnSaveEvent, value);
            remove => RemoveHandler(OnSaveEvent, value);
        }

        public static readonly RoutedEvent OnCancelEvent = EventManager.RegisterRoutedEvent("OnCancel", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AccountEditDialog));
        public event RoutedEventHandler OnCancel
        {
            add => AddHandler(OnCancelEvent, value);
            remove => RemoveHandler(OnCancelEvent, value);
        }

        Account AccountDataContext => DataContext as Account;

        public Account Account
        {
            get
            {
                if (AccountDataContext != null)
                {
                    AccountDataContext.Password = PasswordTB.Password;
                    AccountDataContext.AccountType = (AccountType)AccountTypeCB.SelectedIndex;
                }
                return AccountDataContext;
            }
            set
            {
                DataContext = value;
                if (value != null)
                {
                    PasswordTB.Password = value.Password;
                    AccountTypeCB.SelectedIndex = (byte)value.AccountType;
                }
            }
        }

        public AccountEditDialog()
        {
            InitializeComponent();
        }

        void AccountTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShowAccountChangeAlert)
                if ((!(AccountTypeCB.Tag is bool) || (bool)AccountTypeCB.Tag) &&
                    e.RemovedItems.Count > 0 &&
                    MessageBox.Show("Внимание!\n" +
                    "Изменение приведет к невозможности авторизации до подтверждения администратором вашей должности.\n" +
                    "Продолжить?", "Изменение должности", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    AccountTypeCB.Tag = false;
                    AccountTypeCB.SelectedItem = e.RemovedItems[0];
                    return;
                }
                else
                    AccountTypeCB.Tag = true;
        }

        void ProfileSaveButton_Click(object sender, RoutedEventArgs e)
        {
            AccountDataContext.Modified = true;
            RaiseEvent(new RoutedEventArgs(OnSaveEvent));
        }

        void ProfileCancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnCancelEvent));
        }
    }
}
