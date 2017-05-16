using InCollege.Client.UI.AccountsUI;
using InCollege.Client.UI.ChatUI;
using InCollege.Client.UI.DictionariesUI;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Linq;

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
            CurrentAccountItemHeaderTB.Text = (ProfileDialog.Account = App.Account = await NetworkUtils.WhoAmI())?.FullName;

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

            var statementsData = await NetworkUtils.RequestData<Statement>(this);

            var specialtiesData = await NetworkUtils.RequestData<Specialty>(this);
            var groupsData = await NetworkUtils.RequestData<Group>(this);
            var subjectsData = await NetworkUtils.RequestData<Subject>(this);

            foreach (var current in statementsData)
            {
                current.Group = groupsData.FirstOrDefault(c => c.ID == current.GroupID);
                if (current.Group != null)
                    current.Specialty = specialtiesData.FirstOrDefault(c => c.ID == current.Group.SpecialtyID);

                current.Subject = subjectsData.FirstOrDefault(c => c.ID == current.SubjectID);
            }
            EditStatementDialog.SpecialtyCB.ItemsSource = specialtiesData;
            EditStatementDialog.GroupCB.ItemsSource = groupsData;
            EditStatementDialog.SubjectCB.ItemsSource = subjectsData;

            StatementsLV.ItemsSource = statementsData;
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
            if (StatementsLV.SelectedItem != null)
            {
                EditStatementDialog.AddMode = false;
                EditStatementDialog.Statement = (Statement)StatementsLV.SelectedItem;
                EditStatementDialog.IsOpen = true;
            }
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (Statement current in StatementsLV.SelectedItems)
                await RemoveStatement(current);
            await UpdateData();
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


        async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var statement = new Statement();
            //Attention                    'HERE. It cannot be optimized that way, you thought about.
            statement.ID = int.Parse(await NetworkUtils.ExecuteDataAction<Statement>(this, statement, DataAction.Save));
            statement.IsLocal = false;
            EditStatementDialog.Statement = statement;
            EditStatementDialog.AddMode = true;
            EditStatementDialog.IsOpen = true;
        }

        async void EditStatementDialog_OnSave(object sender, RoutedEventArgs e)
        {
            await NetworkUtils.ExecuteDataAction<Statement>(this, EditStatementDialog.Statement, DataAction.Save);
            EditStatementDialog.IsOpen = false;
        }

        async void EditStatementDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            EditStatementDialog.IsOpen = false;
            if (EditStatementDialog.AddMode)
                await RemoveStatement(EditStatementDialog.Statement);
            await UpdateData();
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

        private void MessagesButton_Click(object sender, RoutedEventArgs e)
        {
            new ChatWindow().ShowDialog();
        }

        async Task RemoveStatement(Statement statement)
        {
            await NetworkUtils.ExecuteDataAction<Statement>(null, statement, DataAction.Remove);
            await NetworkUtils.RemoveWhere<StatementAttestationType>(null, (nameof(StatementAttestationType.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<CommissionMember>(null, (nameof(CommissionMember.StatementID), statement.ID));

            await NetworkUtils.RemoveWhere<MiddleStatementResult>(null, (nameof(MiddleStatementResult.MiddleStatementID), statement.ID));
            await NetworkUtils.RemoveWhere<MiddleStatementResult>(null, (nameof(MiddleStatementResult.QualificationStatementID), statement.ID));

            await NetworkUtils.RemoveWhere<ExamStatementResult>(null, (nameof(ExamStatementResult.ExamStatementID), statement.ID));
        }
    }
}
