using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Windows;
using System.Windows.Controls;

namespace InCollege.UI.AuthorizationUI
{
    public partial class AccountEditDialog : Grid
    {
        public static DependencyProperty ShowAccountChangeAlertProperty =
            DependencyProperty.Register("ShowAccountChangeAlert", typeof(Account), typeof(AccountEditDialog));
        public bool ShowAccountChangeAlert { get; set; }

        public static readonly RoutedEvent OnSaveEvent = EventManager.RegisterRoutedEvent("OnSave", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AccountEditDialog));
        public event RoutedEventHandler OnSave
        {
            add { AddHandler(OnSaveEvent, value); }
            remove { RemoveHandler(OnSaveEvent, value); }
        }

        public static readonly RoutedEvent OnCancelEvent = EventManager.RegisterRoutedEvent("OnCancel", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AccountEditDialog));
        public event RoutedEventHandler OnCancel
        {
            add { AddHandler(OnCancelEvent, value); }
            remove { RemoveHandler(OnCancelEvent, value); }
        }

        private Account _account;

        public Account Account
        {
            get
            {
                if (_account != null)
                {
                    _account.FullName = FullNameTB.Text;
                    _account.UserName = UserNameTB.Text;
                    _account.Password = PasswordTB.Password;
                    _account.BirthDate = BirthDateTB.SelectedDate;
                    _account.AccountType = (AccountType)AccountTypeCB.SelectedIndex;
                }
                return _account;
            }
            set
            {
                _account = value;
                if (value != null)
                {
                    FullNameTB.Text = value.FullName;
                    UserNameTB.Text = value.UserName;
                    PasswordTB.Password = value.Password;
                    BirthDateTB.SelectedDate = value.BirthDate;
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
            _account.Modified = true;
            RaiseEvent(new RoutedEventArgs(OnSaveEvent));
        }

        void ProfileCancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnCancelEvent));
        }
    }
}
