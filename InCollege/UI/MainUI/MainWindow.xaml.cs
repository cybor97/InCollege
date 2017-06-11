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
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System;

namespace InCollege.Client.UI.MainUI
{
    public partial class MainWindow : Window, IUpdatable
    {
        public MainWindow()
        {
            InitializeComponent();

            EditStatementDialog.OnSave += EditStatementDialog_OnSave;
            EditStatementDialog.OnCancel += EditStatementDialog_OnCancel;

            ShowInfo(InfoTag("Base"));
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

            IsEnabled = false;
            Cursor = Cursors.Wait;

            switch (App.Account.AccountType)
            {
                case AccountType.Guest:
                    FileItem.IsEnabled = false;
                    OutputItem.IsEnabled = false;
                    DictionariesItem.IsEnabled = false;
                    ParticipantsItem.IsEnabled = false;
                    StatementsPanel.Visibility = Visibility.Collapsed;
                    break;

                case AccountType.Student:
                    FileItem.IsEnabled = false;
                    OutputItem.IsEnabled = false;
                    DictionariesItem.IsEnabled = false;
                    ParticipantsItem.IsEnabled = false;
                    StatementsPanel.Visibility = Visibility.Collapsed;
                    MarksPanel.Visibility = Visibility.Visible;
                    StatementResultsLV.ItemsSource =
                        (await NetworkUtils.RequestStudyResults())
                        .OrderBy(c => c.StatementResultDate)
                        .GroupBy(c => c.SubjectName_STUDENT_MODE);
                    break;

                case AccountType.Professor:
                    DictionariesItem.IsEnabled = false;
                    ParticipantsItem.IsEnabled = false;
                    await UpdateStatementsData();
                    break;

                case AccountType.DepartmentHead:
                    ParticipantsItem.IsEnabled = false;
                    await UpdateStatementsData();
                    break;

                case AccountType.Admin:
                    await UpdateStatementsData();
                    break;
            }

            Cursor = Cursors.Arrow;
            IsEnabled = true;
        }

        public async Task UpdateStatementsData()
        {
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

            foreach (var current in subjectsData)
                current.Specialty = specialtiesData.FirstOrDefault(c => c.ID == current.SpecialtyID);

            EditStatementDialog.SpecialtyCB.ItemsSource = specialtiesData;
            EditStatementDialog.GroupCB.ItemsSource = groupsData;

            EditStatementDialog.SubjectCB.ItemsSource = subjectsData;

            StatementsLV.ItemsSource = statementsData;
        }

        async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                await UpdateData();
        }

        #region Accounts operations
        void ProfileDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ProfileDialog.IsOpen = false;
        }

        void CurrentAccountItem_Click(object sender, RoutedEventArgs e)
        {
            ProfileDialog.IsOpen = true;
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

        async Task AccountExit()
        {
            App.Token = null;
            await NetworkUtils.Disconnect();
            Process.Start(Assembly.GetExecutingAssembly().Location);
            Process.GetCurrentProcess().Kill();
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
            if (!ChatWindow.Opened)
                new ChatWindow().Show();
            MainMenu.IsOpen = false;
        }
        #endregion
        #region Statement operations
        async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await AddStatement(false, false);
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

        async void EditStatementDialog_OnSave(object sender, RoutedEventArgs e)
        {
            await NetworkUtils.ExecuteDataAction<Statement>(this, EditStatementDialog.Statement, DataAction.Save);
        }

        async void EditStatementDialog_OnCancel(object sender, RoutedEventArgs e)
        {
            EditStatementDialog.IsOpen = false;
            if (EditStatementDialog.AddMode)
                await RemoveStatement(EditStatementDialog.Statement);
            await UpdateData();
        }


        async Task<Statement> AddStatement(bool generatedComplexMode, bool generatedTotalMode, bool background = false)
        {
            if (!(generatedComplexMode && generatedTotalMode))
            {
                var statement = new Statement();
                //Attention                    'HERE. It cannot be optimized that way, you thought about.
                statement.StatementNumber = statement.ID = int.Parse(await NetworkUtils.ExecuteDataAction<Statement>(background ? null : this, statement, DataAction.Save));
                statement.IsLocal = false;
                if (!background)
                    await EditStatementDialog.Show(statement, true, generatedComplexMode, generatedTotalMode);
                return statement;
            }
            else return null;
        }

        async Task RemoveStatement(Statement statement)
        {
            await NetworkUtils.ExecuteDataAction<Statement>(null, statement, DataAction.Remove);
            await NetworkUtils.RemoveWhere<StatementAttestationType>(null, (nameof(StatementAttestationType.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<CommissionMember>(null, (nameof(CommissionMember.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<StatementResult>(null, (nameof(StatementResult.StatementID), statement.ID));
            await NetworkUtils.RemoveWhere<RePass>(null, (nameof(RePass.StatementID), statement.ID));
        }
        #endregion
        #region Generate statement
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
                    var statement = await AddStatement(complexMode, totalMode, true);
                    if (statement != null && statement.ID != -1)
                    {
                        Cursor = Cursors.Wait;

                        var data = new List<StatementResult>();
                        foreach (Statement current in StatementsLV.SelectedItems)
                            data.AddRange(await NetworkUtils.RequestData<StatementResult>(this, strict: true, orAll: true, preserveContext: true, column: null,
                                                                                       whereParams: (name: nameof(StatementResult.StatementID), value: current.ID)));
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

                        Cursor = Cursors.Arrow;

                        await EditStatementDialog.Show(statement, true, complexMode, totalMode);
                        await EditStatementDialog.UpdateData();
                    }
                    await UpdateData();
                }
        }
        #endregion
        #region Filter
        void StatementTypeFilterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        void StatementNumberFilterTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string futureText = StatementNumberFilterTB.Text + e.Text;
            e.Handled = string.IsNullOrWhiteSpace(futureText) || !System.Text.RegularExpressions.Regex.IsMatch(futureText, "^\\d{1,10}$");
        }

        void StatementNumberFilterTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        void EnableFiltersCB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            if (StatementsLV != null)
                StatementsLV.Items.Filter = c =>
                {
                    var statement = (Statement)c;
                    return !(EnableFiltersCB.IsChecked ?? false) ||
                            statement.StatementNumber.ToString().Contains(StatementNumberFilterTB.Text) &&
                           (StatementTypeFilterCB.SelectedIndex == 0 || statement.StatementType == (StatementType)(StatementTypeFilterCB.SelectedIndex - 1));
                };
        }

        #endregion
        #region Help
        void HelpItem_Click(object sender, RoutedEventArgs e)
        {
            HelpPanel.Visibility = Visibility.Visible;
            AddButton.Visibility = Visibility.Collapsed;
        }

        void CloseHelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpPanel.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;
        }

        void HelpCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button buttonSender)
                ShowInfo(InfoTag(buttonSender.Name.Split(new[] { "HelpButton" }, StringSplitOptions.RemoveEmptyEntries)[0]));
        }

        void ShowInfo((string open, string close) tag)
        {
            HelpContentTB.Text = Properties.Resources.MainWindow_Help
                                           .Split(new[] { tag.open }, StringSplitOptions.RemoveEmptyEntries)[1]
                                           .Split(new[] { tag.close }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        (string open, string close) InfoTag(string name)
        {
            return ($"[{name}]", $"[/{name}]");
        }
        #endregion
        #region Other menu items
        async void DictionariesItem_Click(object sender, RoutedEventArgs e)
        {
            int itemIndex = ((MenuItem)(((MenuItem)sender).Parent)).Items.IndexOf(sender);
            if (itemIndex == 0) new AttestationTypesWindow().ShowDialog();
            else new StudyObjectsWindow(itemIndex - 1).ShowDialog();
            await UpdateData();
        }

        async void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        async void CheckVersionItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(await NetworkUtils.CheckVersion());
        }

        async void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Токен авторизации|*.token|Конфигурация клиента|*.json",
                InitialDirectory = CommonVariables.DataDirectory
            };
            if (dialog.ShowDialog() ?? false)
                if (dialog.FileName.EndsWith(".token"))
                    App.Token = File.ReadAllText(dialog.FileName);
                else if (dialog.FileName.EndsWith(".json"))
                    ClientConfiguration.Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientConfiguration>(File.ReadAllText(dialog.FileName));
            await UpdateData();
        }

        void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        void LogItem_Click(object sender, RoutedEventArgs e)
        {
            new LogWindow().ShowDialog();
        }
        #endregion
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