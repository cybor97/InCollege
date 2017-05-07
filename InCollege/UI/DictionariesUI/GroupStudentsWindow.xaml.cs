using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InCollege.Client.UI.DictionariesUI
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
            StudentsLV.ItemsSource = accountData.Where(c => c.GroupID == Group.ID);
            StudentCB.ItemsSource = accountData.Where(c => c.GroupID == -1);
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (Account current in StudentsLV.SelectedItems)
            {
                current.GroupID = -1;
                await NetworkUtils.ExecuteDataAction<Account>(null, current, DataAction.Save);
                StudentDialog.IsOpen = false;
            }
            await UpdateData();
        }

        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentCB.SelectedItem != null)
            {
                var selectedRecord = (Account)StudentCB.SelectedItem;
                selectedRecord.GroupID = Group.ID;
                await NetworkUtils.ExecuteDataAction<Account>(this, selectedRecord, DataAction.Save);

                var contextRecord = (Account)StudentDialog.DataContext;
                if (contextRecord != null)
                {
                    contextRecord.GroupID = -1;
                    await NetworkUtils.ExecuteDataAction<Account>(this, contextRecord, DataAction.Save);
                }
                StudentDialog.IsOpen = false;
            }
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            StudentDialog.IsOpen = false;
        }

        private void StudentDialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsLV.SelectedItem != null && !StudentCB.Items.IsEmpty)
            {
                StudentDialog.DataContext = StudentCB.SelectedItem = StudentsLV.SelectedItem;
                StudentDialog.IsOpen = true;
            }
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!StudentCB.Items.IsEmpty)
            {
                StudentDialog.DataContext = null;
                StudentCB.SelectedItem = StudentCB.Items[00];
                StudentDialog.IsOpen = true;
            }
        }

        async void GroupStudentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }
    }
}
