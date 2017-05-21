using InCollege.Core.Data;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Data;
using System;
using System.Globalization;
using InCollege.Core.Data.Base;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class EditStatementDialog : DialogHost, IUpdatable
    {
        public event RoutedEventHandler OnSave;
        public event RoutedEventHandler OnCancel;

        public static DependencyProperty StatementProperty = DependencyProperty.Register("Statement", typeof(Statement), typeof(EditStatementDialog));
        public Statement Statement
        {
            get => (Statement)GetValue(StatementProperty);
            set => SetValue(StatementProperty, DataContext = value);
        }

        public bool AddMode { get; set; }

        public EditStatementDialog()
        {
            InitializeComponent();

            for (int i = 1; i <= 12; i++)
                SemesterCB.Items.Add(i);
            for (int i = 1; i <= 6; i++)
                CourseCB.Items.Add(i);
        }

        #region Updaters
        public void UpdateGroupList()
        {
            if (SpecialtyCB.SelectedItem == null)
                GroupCB.Visibility = Visibility.Collapsed;
            else
            {
                if (GroupCB.Items != null && (GroupCB.SelectedItem == null || ((Group)GroupCB.SelectedItem).SpecialtyID != ((Specialty)SpecialtyCB.SelectedItem).ID))
                    GroupCB.Items.Filter = c => (((Group)c)?.SpecialtyID ?? -1) == (((Specialty)SpecialtyCB.SelectedItem)?.ID ?? -1);
                GroupCB.Visibility = Visibility.Visible;
            }
        }

        public async Task UpdateData()
        {

            if (GroupCB.SelectedItem != null)
            {
                var studentsData = await NetworkUtils.RequestData<Account>(null, (nameof(Account.AccountType), AccountType.Student), (nameof(Account.GroupID), ((Group)GroupCB.SelectedItem).ID));

                var statementResultsData = await NetworkUtils.RequestData<StatementResult>(null, (nameof(StatementResult.StatementID), Statement.ID));
                foreach (var current in statementResultsData)
                    current.Student = studentsData.FirstOrDefault(c => c.ID == current.StudentID);


                if (GroupCB.SelectedItem != null)
                    StudentCB.ItemsSource = studentsData.Where(currentStudent => !statementResultsData.Any(c => c.StudentID == currentStudent.ID)).ToList();

                StatementResultsLV.ItemsSource = statementResultsData;
            }

            GroupCB.IsEnabled = StatementTypeCB.IsEnabled = SubjectCB.IsEnabled = SpecialtyCB.IsEnabled = StatementResultsLV.Items.Count == 0;

            bool canContainResults = SpecialtyCB.SelectedItem != null && GroupCB.SelectedItem != null && SubjectCB.SelectedItem != null;
            StatementResultsContainer.Visibility = canContainResults ? Visibility.Visible : Visibility.Collapsed;
            UnfilledBlankResults.Visibility = canContainResults ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Selection callbacks
        async void GroupCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateData();
        }

        void SpecialtyCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGroupList();
        }

        void StatementTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatementTypeCB.SelectedIndex == 0)
            {
                MiddleStatementResultsContainer.Visibility = Visibility.Visible;
                ComplexStatementPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MiddleStatementResultsContainer.Visibility = Visibility.Collapsed;
                ComplexStatementPanel.Visibility = Visibility.Visible;
            }
        }
        #endregion
        #region Separate windows
        void CommissionMembersButton_Click(object sender, RoutedEventArgs e)
        {
            new StatementCommissionMembersWindow(Statement).ShowDialog();
        }

        void AttestationTypesButton_Click(object sender, RoutedEventArgs e)
        {
            new StatementAttestationTypesWindow(Statement).ShowDialog();
        }

        void SeparateWindowButton_Click(object sender, RoutedEventArgs e)
        {
            new StatementResultsWindow(Statement).ShowDialog();
        }
        #endregion
        #region Middle statement results list
        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectCB.SelectedItem != null)
            {
                MiddleStatementResultDialog.DataContext = new StatementResult
                {
                    StatementID = Statement.ID,
                    SubjectID = ((Subject)SubjectCB.SelectedItem).ID,
                };
                MiddleStatementResultDialog.IsOpen = true;
            }
            else MessageBox.Show("Выберите дисциплину!");
        }

        void EditStatementResultItem_Click(object sender, RoutedEventArgs e)
        {
            if (StatementResultsLV.SelectedItem != null)
            {
                StudentCB.ItemsSource = new[] { ((StatementResult)StatementResultsLV.SelectedItem).Student };
                MiddleStatementResultDialog.DataContext = StatementResultsLV.SelectedItem;
                MiddleStatementResultDialog.IsOpen = true;
            }
        }

        async void RemoveStatementResultItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var current in StatementResultsLV.SelectedItems)
                await NetworkUtils.ExecuteDataAction<StatementResult>(null, (DBRecord)current, DataAction.Remove);
            await UpdateData();
        }

        async void SaveMiddleStatementButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentCB.SelectedItem != null && !string.IsNullOrEmpty(MarkTB.Text))
                await NetworkUtils.ExecuteDataAction<StatementResult>(this, (DBRecord)MiddleStatementResultDialog.DataContext, DataAction.Save);
            MiddleStatementResultDialog.IsOpen = false;
        }

        async void CancelMiddleStatementButton_Click(object sender, RoutedEventArgs e)
        {
            MiddleStatementResultDialog.IsOpen = false;
            await UpdateData();
        }
        #endregion
        #region Text filters
        void MarkTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string futureText = MarkTB.Text + e.Text;
            e.Handled = string.IsNullOrWhiteSpace(futureText) || !System.Text.RegularExpressions.Regex.IsMatch(futureText, "^[1-5]$");
        }

        void StatementNumberTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string futureText = StatementNumberTB.Text + e.Text;
            e.Handled = string.IsNullOrWhiteSpace(futureText) || !System.Text.RegularExpressions.Regex.IsMatch(futureText, "^\\d{1,10}$");
        }

        void StatementNumberTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        void MarkTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        #endregion
        #region Dialog buttons
        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave?.Invoke(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke(sender, e);
        }

        void PrintButton_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }

    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            var item = (ListViewItem)value;
            return ItemsControl.ItemsControlFromItemContainer(item).ItemContainerGenerator.IndexFromContainer(item) + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatementTypeToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(StatementType)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (StatementType)(int)value;
        }
    }
}
