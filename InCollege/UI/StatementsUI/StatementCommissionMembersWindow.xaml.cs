using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class StatementCommissionMembersWindow : Window, IUpdatable
    {
        Statement Statement { get; set; }
        public StatementCommissionMembersWindow(Statement statement)
        {
            InitializeComponent();
            Statement = statement;
        }

        async void StatementCommissionMembersWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            var professorsData = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.Professor));
            professorsData.AddRange(await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead)));

            var commissionMembersData = await NetworkUtils.RequestData<CommissionMember>(this, (nameof(CommissionMember.StatementID), Statement?.ID ?? -1));
            foreach (var current in commissionMembersData)
                current.Professor = professorsData.FirstOrDefault(c => c.ID == current.ProfessorID);
            CommissionMembersLV.ItemsSource = commissionMembersData;

            ProfessorCB.ItemsSource = professorsData.Where(currentProfessor => !commissionMembersData.Any(c => c.ProfessorID == currentProfessor.ID));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ProfessorCB.Items.IsEmpty)
            {
                CommissionMemberDialog.DataContext = new CommissionMember { StatementID = Statement?.ID ?? -1 };
                CommissionMemberDialog.IsOpen = true;
            }

        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (CommissionMembersLV.SelectedItem != null)
            {
                CommissionMemberDialog.DataContext = CommissionMembersLV.SelectedItem;
                CommissionMemberDialog.IsOpen = true;
            }
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var current in CommissionMembersLV.SelectedItems)
                await NetworkUtils.ExecuteDataAction<CommissionMember>(null, (DBRecord)current, DataAction.Remove);
            await UpdateData();
        }

        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfessorCB.SelectedItem != null)
                await NetworkUtils.ExecuteDataAction<CommissionMember>(this, (DBRecord)CommissionMemberDialog.DataContext, DataAction.Save);
            CommissionMemberDialog.IsOpen = false;
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CommissionMemberDialog.IsOpen = false;
        }

        async void CommissionMemberDialog_KeyDown(object sender, KeyEventArgs e)
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
