using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InCollege.UI
{
    public partial class StudyObjectsWindow : Window, IUpdatable
    {
        Dictionary<object, (DialogHost Dialog, ListView ListView)> Views;

        public StudyObjectsWindow(int tabIndex)
        {
            InitializeComponent();
            StudyObjectsTabs.SelectedIndex = tabIndex;

            Views = new Dictionary<object, (DialogHost dialog, ListView list)>
            {
                { "Department", (EditDepartmentDialog, DepartmentsLV) },
                { "Specialty", (EditSpecialtyDialog, SpecialtiesLV) },
                { "Group", (EditGroupDialog, GroupsLV) },
                { "Subject", (EditSubjectDialog, SubjectsLV) }
            };
        }

        public async Task UpdateData()
        {
            Title = ((TabItem)StudyObjectsTabs.SelectedItem).Header.ToString();
            //If not Subjects.
            if (StudyObjectsTabs.SelectedIndex != StudyObjectsTabs.Items.Count - 1)
            {
                var departmentHeads = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead));
                DepartmentHeadCB.ItemsSource = departmentHeads;

                var departmentsData = await NetworkUtils.RequestData<Department>(this);
                DepartmentCB.ItemsSource = departmentsData;

                foreach (Department current in departmentsData)
                    current.DepartmentHead = departmentHeads.FirstOrDefault(c => c.ID == current.DepartmentHeadID);
                DepartmentsLV.ItemsSource = departmentsData;

                if (StudyObjectsTabs.SelectedIndex >= 1)
                {
                    var specialtiesData = await NetworkUtils.RequestData<Specialty>(this);
                    SpecialtyCB.ItemsSource = specialtiesData;

                    foreach (Specialty current in specialtiesData)
                        current.Department = departmentsData.FirstOrDefault(c => c.ID == current.DepartmentID);
                    SpecialtiesLV.ItemsSource = specialtiesData;

                    if (StudyObjectsTabs.SelectedIndex >= 2)
                    {
                        var groupsData = await NetworkUtils.RequestData<Group>(this);
                        foreach (Group current in groupsData)
                            current.Specialty = specialtiesData.FirstOrDefault(c => c.ID == current.SpecialtyID);
                        GroupsLV.ItemsSource = groupsData;
                    }
                }
            }
            else SubjectsLV.ItemsSource = await NetworkUtils.RequestData<Subject>(this);
        }

        async void StudyObjectsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateData();
        }

        async void StudyObjectsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Views[((TabItem)StudyObjectsTabs.SelectedItem).Tag].Dialog;
            dialog.IsOpen = true;
            dialog.DataContext =
                dialog == EditDepartmentDialog ? new Department() :
                dialog == EditSpecialtyDialog ? new Specialty() :
                dialog == EditGroupDialog ? new Group() :
                //WHY?? WHY THE HELL I HAD TO DO THIS??? WHY IT CANNOT DETERMINATE MINIMAL DERIVABLE TYPE???
                dialog == EditSubjectDialog ? (DBRecord)new Subject() :
                null;
        }

        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveButton = (Button)sender;
            var dialog = Views[saveButton.Tag].Dialog;
            dialog.IsOpen = false;
            await NetworkUtils.ExecuteDataAction(saveButton.Tag.ToString(), this, (DBRecord)dialog.DataContext, DataAction.Save);
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Views[((Button)sender).Tag].Dialog.IsOpen = false;
        }

        void Something_SelectionChanged_Ignore(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        void EditItem_Click(object sender, RoutedEventArgs e)
        {
            var view = Views[((TabItem)StudyObjectsTabs.SelectedItem).Tag];
            var dialog = view.Dialog;
            dialog.DataContext = view.ListView.SelectedItem;
            dialog.IsOpen = true;
        }

        async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((TabItem)StudyObjectsTabs.SelectedItem).Tag.ToString();
            foreach (DBRecord current in Views[tag].ListView.SelectedItems)
                await NetworkUtils.ExecuteDataAction(tag, null, current, DataAction.Remove);
            await UpdateData();
        }

        async void TeachersItem_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsLV.SelectedItem != null)
            {
                new SubjectTeachersWindow((Subject)SubjectsLV.SelectedItem).ShowDialog();
                await UpdateData();
            }
        }

        async void GroupStudentsItem_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsLV.SelectedItem != null)
            {
                new GroupStudentsWindow((Group)GroupsLV.SelectedItem).ShowDialog();
                await UpdateData();
            }
        }
    }
}
