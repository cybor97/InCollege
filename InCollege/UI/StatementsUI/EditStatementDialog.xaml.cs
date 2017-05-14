using InCollege.Core.Data;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Threading.Tasks;

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
            set => SetValue(StatementProperty, value);
        }

        public EditStatementDialog()
        {
            InitializeComponent();

            for (int i = 1; i <= 12; i++)
                SemesterCB.Items.Add(i);
            for (int i = 1; i <= 6; i++)
                CourseCB.Items.Add(i);
        }

        public async Task UpdateData()
        {
            SubjectCB.ItemsSource = await NetworkUtils.RequestData<Subject>(null);
            SpecialtyCB.ItemsSource = await NetworkUtils.RequestData<Specialty>(null);
            UpdateGroupList();
        }

        async void UpdateGroupList()
        {
            GroupCB.ItemsSource = SpecialtyCB.SelectedItem != null ?
                await NetworkUtils.RequestData<Group>(null, (nameof(Group.SpecialtyID), ((Specialty)SpecialtyCB.SelectedItem).ID)) :
                null;
        }

        async void EditStatementDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
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
            new StatementCommissionMembersWindow((Statement)DataContext).ShowDialog();
        }

        void AttestationTypesButton_Click(object sender, RoutedEventArgs e)
        {
            new StatementAttestationTypesWindow((Statement)DataContext).ShowDialog();
        }

        void SpecialtyCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateGroupList();
        }
    }
}
