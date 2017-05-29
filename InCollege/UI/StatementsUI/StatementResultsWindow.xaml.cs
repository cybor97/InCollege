using InCollege.Core.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static InCollege.Client.UI.Util.Generators.StatementViewModelGeneratorFactory;
using System.Threading.Tasks;
using InCollege.Core.Data.Base;
using InCollege.Client.UI.Util.Generators;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Input;
using System;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class StatementResultsWindow : Window, IUpdatable
    {
        Statement Statement { get => (Statement)DataContext; set => DataContext = value; }
        List<StatementResult> StatementResults;
        StatementResultViewModelGenerator Generator;

        public StatementResultsWindow(Statement statement)
        {
            InitializeComponent();
            Statement = statement;
            Title = $"Содержимое ведомости от {statement?.StatementDate.ToString("dd MMMM yyyy")}";

            Generator = GetGenerator((statement?.StatementType ?? StatementType.CourceProject) == StatementType.Total ? GeneratorType.TotalStatement : GeneratorType.ComplexStatement);
        }

        async void StatementResultsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            var subjectData = await NetworkUtils.RequestData<Subject>(this);
            var studentData = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.Student), (nameof(Account.GroupID), Statement?.GroupID));

            StatementResults = await NetworkUtils.RequestData<StatementResult>(this, (nameof(StatementResult.StatementID), Statement?.ID));
            foreach (var current in StatementResults)
            {
                current.Subject = subjectData?.FirstOrDefault(c => c.ID == current.SubjectID);
                current.Student = studentData?.FirstOrDefault(c => c.ID == current.StudentID);
            }

            var columns = Generator.GetColumns(StatementResults);

            ContentGridView.Columns.Clear();
            foreach (var current in columns)
                if (!string.IsNullOrWhiteSpace(current.uiName))
                {
                    var header = new GridViewColumnHeader { Content = current.uiName, Tag = current.name };
                    header.MouseDoubleClick += Header_MouseDoubleClick;
                    ContentGridView.Columns.Add(new GridViewColumn
                    {
                        Header = header,
                        DisplayMemberBinding = new Binding(current.name)
                    });
                }

            ContentLV.ItemsSource = Generator.GetResults(Generator.GetColumns(StatementResults).Select(c => c.name), StatementResults);

            StudentCB.ItemsSource = studentData;
            SubjectCB.ItemsSource = subjectData;
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            StatementResultDialog.DataContext = new StatementResult { StatementID = Statement?.ID ?? -1 };
            StatementResultDialog.IsOpen = true;
        }

        async void SaveStatementButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentCB.SelectedItem != null && SubjectCB.SelectedItem != null && MarkCB.SelectedItem != null)
            {
                Focus();
                var dataContext = (StatementResult)StatementResultDialog.DataContext;
                if (dataContext.IsLocal)
                {
                    var sameRecord = StatementResults?.FirstOrDefault(c => c.StudentID == dataContext.StudentID && c.SubjectID == dataContext.SubjectID);
                    if (sameRecord != null)
                    {
                        dataContext.ID = sameRecord.ID;
                        dataContext.IsLocal = false;
                    }
                }

                dataContext.MarkValue = MarkCB.SelectedIndex >= 0 && MarkCB.SelectedIndex < 4 ? (sbyte)(MarkCB.SelectedIndex + 2) :
                    (sbyte)(TechnicalMarkValue)Enum.Parse(typeof(TechnicalMarkValue), ((ComboBoxItem)MarkCB.SelectedItem).Name.Split(new[] { "Item" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                await NetworkUtils.ExecuteDataAction<StatementResult>(this, dataContext, DataAction.Save);
                StatementResultDialog.IsOpen = false;
            }
        }

        async void CancelStatementButton_Click(object sender, RoutedEventArgs e)
        {
            StatementResultDialog.IsOpen = false;
            await UpdateData();
        }

        void EditStatementResultItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ContentLV.SelectedItem != null)
            {
                var studentID = ((StatementResultViewModel)ContentLV.SelectedItem).StudentID;
                EditStatementResultItem.DisplayMemberPath = nameof(StatementResult.SubjectName);
                EditStatementResultItem.Items?.Clear();

                StatementResults.Where(c => c.StudentID == studentID)
                                .ToList()
                                .ForEach(c =>
                                {
                                    var menuItem = new MenuItem { Header = $"{c.SubjectIndex} ({c.SubjectName})", Tag = c.ID };
                                    menuItem.Click += EditStatementResultItem_Click;
                                    EditStatementResultItem.Items.Add(menuItem);
                                });
            }
            else EditStatementResultItem.Items.Clear();
        }

        void EditStatementResultItem_Click(object sender, RoutedEventArgs e)
        {
            if (ContentLV.SelectedItem != null)
            {
                var tag = ((MenuItem)sender).Tag;
                if (tag is int tagInt && tagInt != -1)
                {
                    var item = StatementResults.FirstOrDefault(c => c.ID == tagInt);
                    if (item != null)
                    {
                        StatementResultDialog.DataContext = item;
                        StatementResultDialog.IsOpen = true;
                    }
                }
            }
        }

        void RemoveStatementResultItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ContentLV.SelectedItem != null)
            {
                var studentID = ((StatementResultViewModel)ContentLV.SelectedItem).StudentID;
                RemoveStatementResultItem.DisplayMemberPath = nameof(StatementResult.SubjectName);
                RemoveStatementResultItem?.Items?.Clear();

                var allItem = new MenuItem { Header = "Все", Tag = "*" };
                allItem.Click += RemoveStatementResultItem_Click;
                RemoveStatementResultItem.Items.Add(allItem);

                StatementResults.Where(c => c.StudentID == studentID)
                                .ToList()
                                .ForEach(c =>
                                {
                                    var menuItem = new MenuItem { Header = $"{c.SubjectIndex} ({c.SubjectName})", Tag = c.ID };
                                    menuItem.Click += RemoveStatementResultItem_Click;
                                    RemoveStatementResultItem.Items.Add(menuItem);
                                });
            }
            else RemoveStatementResultItem.Items.Clear();
        }

        async void RemoveStatementResultItem_Click(object sender, RoutedEventArgs e)
        {
            //Double check, but why, the hell, no?)
            if (ContentLV.SelectedItem != null)
            {
                var studentID = ((StatementResultViewModel)ContentLV.SelectedItem).StudentID;
                var tag = ((MenuItem)sender).Tag;
                if (tag != null)
                    if (tag is int tagInt && tagInt != -1)
                    {
                        var item = StatementResults.FirstOrDefault(c => c.ID == tagInt);
                        if (item != null)
                            await NetworkUtils.ExecuteDataAction<StatementResult>(this, item, DataAction.Remove);
                    }
                    else if (tag is string tagString && tagString == "*")
                    {
                        foreach (var current in StatementResults.Where(c => c.StudentID == studentID))
                            await NetworkUtils.ExecuteDataAction<StatementResult>(null, current, DataAction.Remove);
                        await UpdateData();
                    }
            }
        }

        void Header_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Statement.StatementType == StatementType.Total &&
                sender is GridViewColumnHeader header &&
                header.Tag is string tagString &&
                tagString.StartsWith("subject"))

            {
                StatementResultDateTB.SelectedDate = null;
                StatementResultDateDialog.Tag = tagString;
                StatementResultDateDialog.IsOpen = true;
            }

        }

        void CancelStatementDateButton_Click(object sender, RoutedEventArgs e)
        {
            StatementResultDateDialog.IsOpen = false;
        }

        async void SaveStatementDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatementResultDateDialog.Tag is string tagString &&
                int.TryParse(tagString.Split(new[] { "subject" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "", out int subjectID))
            {
                foreach (var current in StatementResults.Where(c => c.SubjectID == subjectID))
                {
                    current.StatementResultDate = StatementResultDateTB.SelectedDate;
                    await NetworkUtils.ExecuteDataAction<StatementResult>(null, current, DataAction.Save);
                }
                await UpdateData();
            }
            StatementResultDateDialog.IsOpen = false;
        }
    }
}
