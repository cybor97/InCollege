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
using System.Linq;
using System;
using System.Collections.Generic;

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
            App.Account = await NetworkUtils.WhoAmI();

            if (App.Account == null || !App.Account.Approved)
            {
                if (App.Account != null)
                    MessageBox.Show("Извините, ваша должность не подтверждена. Обратитесь к администратору.");
                await AccountExit();
            }

            CurrentAccountItemHeaderTB.Text = (ProfileDialog.Account = App.Account)?.FullName;

            //TODO:
            //if(teacher) request<statement>(teacher.subjects);
            //else if (departmentHead) request<statement>(departmentHead.groups);
            //else if (admin) request<statement>(all);
            //On server side!11

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
            if (itemIndex == 0) new AttestationTypesWindow().ShowDialog();
            else new StudyObjectsWindow(itemIndex - 1).ShowDialog();
            await UpdateData();
        }

        async void ParticipantsItem_Click(object sender, RoutedEventArgs e)
        {
            new AccountsWindow((AccountType)(((MenuItem)(((MenuItem)sender).Parent)).Items.IndexOf(sender) + 1)).ShowDialog();
            await UpdateData();
        }

        async void AccountExitItem_Click(object sender, RoutedEventArgs e)
        {
            await AccountExit();
        }

        async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                IsEnabled = false;
                await UpdateData();
                IsEnabled = true;
                Focus();
            }
        }

        async void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (StatementsLV.SelectedItem != null)
                await EditStatementDialog.Show((Statement)StatementsLV.SelectedItem, false, false, false);
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

        async Task AccountExit()
        {
            App.Token = null;
            await NetworkUtils.Disconnect();
            Process.Start(Assembly.GetExecutingAssembly().Location);
            Process.GetCurrentProcess().Kill();
        }


        async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await AddStatement(false, false);
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

        async void ProfileDialog_OnSave(object sender, RoutedEventArgs e)
        {
            if (ProfileDialog.Account != null)
                await NetworkUtils.ExecuteDataAction<Account>(this, ProfileDialog.Account, DataAction.Save);

            ProfileDialog.IsOpen = false;
        }

        void ProfileDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = false;
        }

        void MessagesButton_Click(object sender, RoutedEventArgs e)
        {
            new ChatWindow().ShowDialog();
        }

        async Task RemoveStatement(Statement statement)
        {
            await NetworkUtils.ExecuteDataAction<Statement>(null, statement, DataAction.Remove);
            await NetworkUtils.RemoveWhere<StatementAttestationType>(null, (nameof(StatementAttestationType.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<CommissionMember>(null, (nameof(CommissionMember.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<StatementResult>(null, (nameof(StatementResult.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<RePass>(null, (nameof(RePass.StatementID), statement.ID));
        }

        async void GenerateComplexStatementItem_Click(object sender, RoutedEventArgs e)
        {
            await GenerateStatement(true, false, 1);
        }

        async void GenerateTotalStatementItem_Click(object sender, RoutedEventArgs e)
        {
            await GenerateStatement(false, true, 4);
        }

        async Task GenerateStatement(bool complexMode, bool totalMode, int strangeThreshold)
        {
            if (StatementsLV.SelectedItems.Count > 0)
                if (StatementsLV.SelectedItems.Count > strangeThreshold ||
                    MessageBox.Show($"Имеет ли смысл? Меньше {strangeThreshold + 1} ведомостей.", "Странная операция", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var statement = await AddStatement(complexMode, totalMode, false);
                    if (statement != null && statement.ID != -1)
                    {
                        Cursor = Cursors.Wait;

                        var data = await NetworkUtils.RequestData<StatementResult>(this, strict: true, orAll: true, preserveContext: true, column: null,
                                                                                   whereParams: StatementsLV.SelectedItems
                                                                                   .OfType<Statement>()
                                                                                   .Select(c => (
                                                                                    name: nameof(StatementResult.StatementID),
                                                                                    value: (object)c.ID))
                                                                                   .ToArray());
                        var distinctor = new DistinctByStudentAndSubject();
                        //Latest - at the end
                        data.Sort((c1, c2) => c1.StatementResultDate > c2.StatementResultDate ? 1 : c1.StatementResultDate < c2.StatementResultDate ? -1 : 0);

                        //Get empty student-subject intersections, fill with blanks.
                        //Get last student-subject couples, distinct by distinctor(student-subject)->
                        //set owner as just created statement->
                        //make 'local' to tell the system to re-create it on server side->
                        //save on server->
                        //update data in opened by adding new statement->
                        var studentIDs = data.Select(c => c.StudentID).Distinct();
                        var subjectIDs = data.Select(c => c.SubjectID).Distinct();

                        foreach (var studentID in studentIDs)
                            foreach (var subjectID in subjectIDs)
                                if (!data.Any(c => c.StudentID == studentID && c.SubjectID == subjectID))
                                    await NetworkUtils.ExecuteDataAction<StatementResult>(null, new StatementResult
                                    {
                                        StatementID = statement.ID,
                                        StudentID = studentID,
                                        SubjectID = subjectID,
                                        MarkValue = (sbyte)TechnicalMarkValue.Blank,
                                        StatementResultDate = null,
                                        TicketNumber = -1
                                    }, DataAction.Save);

                        foreach (var current in data.Select(c => (studentID: c.StudentID, subjectID: c.SubjectID))
                                                    .Select(c => data.LastOrDefault(currentResult => currentResult.StudentID == c.studentID &&
                                                                                                     currentResult.SubjectID == c.subjectID))
                                                    .Distinct(distinctor))
                        {
                            current.StatementID = statement.ID;
                            current.IsLocal = true;
                            await NetworkUtils.ExecuteDataAction<StatementResult>(null, current, DataAction.Save);
                        }

                        await EditStatementDialog.UpdateData();
                        Cursor = Cursors.Arrow;

                        await EditStatementDialog.Show(statement, true, complexMode, totalMode);
                    }
                }
        }

        async Task<Statement> AddStatement(bool generatedComplexMode, bool generatedTotalMode, bool showDialog = true)
        {
            if (!(generatedComplexMode && generatedTotalMode))
            {
                var statement = new Statement();
                //Attention                    'HERE. It cannot be optimized that way, you thought about.
                statement.ID = int.Parse(await NetworkUtils.ExecuteDataAction<Statement>(this, statement, DataAction.Save));
                statement.IsLocal = false;
                if (showDialog)
                    await EditStatementDialog.Show(statement, true, generatedComplexMode, generatedTotalMode);
                return statement;
            }
            else return null;
        }
    }

    class DistinctByStudentAndSubject : IEqualityComparer<StatementResult>
    {
        public bool Equals(StatementResult x, StatementResult y)
        {
            return x.StudentID == y.StudentID &&
                   x.SubjectID == y.SubjectID;
        }

        public int GetHashCode(StatementResult obj)
        {
            return int.Parse($"{obj.StudentID}{obj.SubjectID}");
        }
    }
}
