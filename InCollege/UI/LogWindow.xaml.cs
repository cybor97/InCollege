using InCollege.Core.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InCollege.Client.UI
{
    public partial class LogWindow : Window, IUpdatable
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        async void LogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            IsEnabled = false;
            var logData = await NetworkUtils.RequestData<Log>(this);
            if (logData != null)
                foreach (var current in logData)
                    current.User = (await NetworkUtils.RequestData<Account>(this, (nameof(Account.ID), current.AccountID)))?.FirstOrDefault();
            LogLV.ItemsSource = logData;
            IsEnabled = true;
        }

        void LogLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LogLV.SelectedItem != null)
            {
                LogContentDialog.DataContext = LogLV.SelectedItem;
                LogContentDialog.IsOpen = true;
            }
        }

        void CloseContentDialogButton_Click(object sender, RoutedEventArgs e)
        {
            LogContentDialog.IsOpen = false;
        }

        async void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить все записи из журнала?", "Очистить", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                await NetworkUtils.RemoveWhere<Log>(this);
        }

        async void LogWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) await UpdateData();
        }

        void LogContentDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) LogContentDialog.IsOpen = false;
        }
    }
}
