using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class RePassesWindow : Window, IUpdatable
    {
        int StatementID { get; set; }
        int StatementResultID { get; set; }
        int SubjectID { get; set; }
        IUpdatable Context { get; set; }

        public RePassesWindow(int statementID, int statementResultID, int subjectID, string studentFullName, IUpdatable context)
        {
            InitializeComponent();
            Title = $"Пересдачи студента {studentFullName}";
            StatementID = statementID;
            StatementResultID = statementResultID;
            SubjectID = subjectID;
        }

        async void RePassesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            var rePassesData = await NetworkUtils.RequestData<RePass>(this, (nameof(RePass.StatementID), StatementID), (nameof(RePass.StatementResultID), StatementResultID));


            var professorsData = (await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.Professor)));
            professorsData.AddRange(await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead)));

            var teachersData = await NetworkUtils.RequestData<Teacher>(this, (nameof(Teacher.SubjectID), SubjectID));
            foreach (var current in teachersData)
                current.Professor = professorsData.FirstOrDefault(c => c.ID == current.ProfessorID);

            TeacherCB.ItemsSource = teachersData;

            foreach (var current in rePassesData)
                current.Teacher = teachersData.FirstOrDefault(c => current.TeacherID == c.ID);

            RePassesLV.ItemsSource = rePassesData;
        }

        async void ReplaceSourceItem_Click(object sender, RoutedEventArgs e)
        {
            if (RePassesLV.SelectedItem != null)
            {
                var source = (await NetworkUtils.RequestData<StatementResult>(this, (nameof(StatementResult.ID), StatementResultID)))?.FirstOrDefault();
                if (source != null)
                {
                    source.MarkValue = ((RePass)RePassesLV.SelectedItem).MarkValue;
                    await NetworkUtils.ExecuteDataAction<StatementResult>(this, source, DataAction.Save);
                    Context?.UpdateData();
                }
            }
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            RePassDialog.DataContext = new RePass
            {
                StatementID = StatementID,
                StatementResultID = StatementResultID,
                RePassDate = DateTime.Now
            };
            RePassDialog.IsOpen = true;
        }

        void EditItem_Click(object sender, RoutedEventArgs e)
        {
            RePassDialog.DataContext = RePassesLV.SelectedItem;
            RePassDialog.IsOpen = true;
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (RePassesLV.SelectedItems.Count > 0)
            {
                foreach (RePass current in RePassesLV.SelectedItems)
                    await NetworkUtils.ExecuteDataAction<RePass>(null, current, DataAction.Remove);
                await UpdateData();
            }
        }

        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MarkCB.SelectedItem != null && TeacherCB.SelectedItem != null)
            {
                var rePass = (RePass)RePassDialog.DataContext;
                rePass.MarkValue = MarkCB.SelectedIndex >= 0 && MarkCB.SelectedIndex < 4 ? (sbyte)(MarkCB.SelectedIndex + 2) :
                (sbyte)(TechnicalMarkValue)Enum.Parse(typeof(TechnicalMarkValue), ((ComboBoxItem)MarkCB.SelectedItem).Name.Split(new[] { "Item" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                await NetworkUtils.ExecuteDataAction<RePass>(this, rePass, DataAction.Save);
            }
            RePassDialog.IsOpen = false;
            await UpdateData();
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RePassDialog.IsOpen = false;
        }

        void RePassDialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        void RePassesWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }
}
