using InCollege.Core.Data;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Data;
using System;
using System.Globalization;
using InCollege.Core.Data.Base;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class EditStatementDialog : DialogHost
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

        public async Task UpdateStudentList()
        {
            if (GroupCB.SelectedItem != null)
                StudentCB.ItemsSource = await NetworkUtils.RequestData<Account>(null, (nameof(Account.AccountType), AccountType.Student), (nameof(Account.GroupID), ((Group)GroupCB.SelectedItem).ID));
        }

        async void GroupCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateStudentList();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave?.Invoke(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke(sender, e);
        }

        void CommissionMembersButton_Click(object sender, RoutedEventArgs e)
        {
            new StatementCommissionMembersWindow(Statement).ShowDialog();
        }

        void AttestationTypesButton_Click(object sender, RoutedEventArgs e)
        {
            new StatementAttestationTypesWindow(Statement).ShowDialog();
        }

        void SpecialtyCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGroupList();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SeparateWindowButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectCB.SelectedItem != null)
            {
                MiddleStatementResultDialog.DataContext = new MiddleStatementResult
                {
                    MiddleStatementID = Statement.ID,
                    SubjectID = ((Subject)SubjectCB.SelectedItem).ID
                };
                MiddleStatementResultDialog.IsOpen = true;
            }
            else MessageBox.Show("Выберите дисциплину!");
        }

        void SaveMiddleStatementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented :(");
        }

        void CancelMiddleStatementButton_Click(object sender, RoutedEventArgs e)
        {
            MiddleStatementResultDialog.IsOpen = false;
        }

        private void MarkTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(MarkTB.Text + e.Text, "^[1-5]$");
        }

    }

    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            var item = (ListViewItem)value;
            return ItemsControl.ItemsControlFromItemContainer(item).ItemContainerGenerator.IndexFromContainer(item);
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
