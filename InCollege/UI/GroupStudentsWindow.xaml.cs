using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InCollege.UI
{
    //TODO:Implement!
    public partial class GroupStudentsWindow : Window, IUpdatable
    {
        Group Group { get; set; }

        public GroupStudentsWindow(Group group)
        {
            InitializeComponent();
            Group = group;
            Title = $"Группа \"{group?.GroupName} {group?.GroupCode}\"";
        }

        public async Task UpdateData()
        {
            var accountData = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.Student));
            StudentsLV.ItemsSource = accountData;
            StudentCB.ItemsSource = accountData;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StudentDialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            StudentDialog.DataContext = ((List<Account>)StudentCB.ItemsSource).FirstOrDefault();
            StudentDialog.IsOpen = true;
        }

        async void GroupStudentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }
    }
}
