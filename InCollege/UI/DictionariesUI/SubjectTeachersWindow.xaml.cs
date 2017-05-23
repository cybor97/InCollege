using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InCollege.Client.UI.DictionariesUI
{
    public partial class SubjectTeachersWindow : Window, IUpdatable
    {
        Subject Subject { get; set; }

        public SubjectTeachersWindow(Subject subject)
        {
            InitializeComponent();
            Subject = subject;
            Title = $"Преподаватели \"{subject?.SubjectName}\"";
        }

        async void SubjectTeachersWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            var professorsData = (await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.Professor)));
            professorsData.AddRange(await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead)));

            var teachersData = await NetworkUtils.RequestData<Teacher>(this, (nameof(Teacher.SubjectID), Subject?.ID ?? -1));
            foreach (var current in teachersData)
                current.Professor = professorsData.FirstOrDefault(c => c.ID == current.ProfessorID);
            SubjectTeachersLV.ItemsSource = teachersData;

            ProfessorCB.ItemsSource = professorsData.Where(currentProfessor => !teachersData.Any(c => c.ProfessorID == currentProfessor.ID));
        }

        void TeacherDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SaveButton_Click(null, null);
            else if (e.Key == Key.Escape)
                CancelButton_Click(null, null);
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ProfessorCB.Items.IsEmpty)
            {
                TeacherDialog.DataContext = new Teacher { SubjectID = Subject?.ID ?? -1 };
                TeacherDialog.IsOpen = true;
            }
        }

        void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectTeachersLV.SelectedItem != null)
            {
                TeacherDialog.DataContext = SubjectTeachersLV.SelectedItem;
                TeacherDialog.IsOpen = true;
            }
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var current in SubjectTeachersLV.SelectedItems)
                await NetworkUtils.ExecuteDataAction<Teacher>(null, (DBRecord)current, DataAction.Remove);
            await UpdateData();
        }

        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfessorCB.SelectedItem != null)
                await NetworkUtils.ExecuteDataAction<Teacher>(this, (DBRecord)TeacherDialog.DataContext, DataAction.Save);
            TeacherDialog.IsOpen = false;
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            TeacherDialog.IsOpen = false;
        }

        async void SubjectTeachersWindow_KeyDown(object sender, KeyEventArgs e)
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