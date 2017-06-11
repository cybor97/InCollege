using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System;
using System.Globalization;

namespace InCollege.Client.UI.DictionariesUI
{
    public partial class StudyObjectsWindow : Window, IUpdatable
    {
        Dictionary<object, (DialogHost Dialog, ListView ListView)> Views;

        public StudyObjectsWindow(int tabIndex)
        {
            InitializeComponent();
            StudyObjectsTabs.SelectedIndex = tabIndex;
            StudyObjectsTabs.SelectionChanged += (s, e) => Title = ((TabItem)StudyObjectsTabs.SelectedItem).Header.ToString();

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
            var departmentHeads = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead));
            DepartmentHeadCB.ItemsSource = departmentHeads;

            var departmentsData = await NetworkUtils.RequestData<Department>(this);
            DepartmentCB.ItemsSource = departmentsData;

            foreach (Department current in departmentsData)
                current.DepartmentHead = departmentHeads.FirstOrDefault(c => c.ID == current.DepartmentHeadID);
            DepartmentsLV.ItemsSource = departmentsData;

            var specialtiesData = await NetworkUtils.RequestData<Specialty>(this);
            SpecialtyCB.ItemsSource = specialtiesData;

            foreach (Specialty current in specialtiesData)
                current.Department = departmentsData.FirstOrDefault(c => c.ID == current.DepartmentID);
            SpecialtiesLV.ItemsSource = specialtiesData;

            var groupsData = await NetworkUtils.RequestData<Group>(this);
            foreach (Group current in groupsData)
                current.Specialty = specialtiesData.FirstOrDefault(c => c.ID == current.SpecialtyID);
            GroupsLV.ItemsSource = groupsData;

            var subjectsData = await NetworkUtils.RequestData<Subject>(this);
            foreach (var current in subjectsData)
                current.Specialty = specialtiesData.FirstOrDefault(c => c.ID == current.SpecialtyID);
            SubjectsLV.ItemsSource = subjectsData;

            SubjectSpecialtyCB.ItemsSource = specialtiesData;
            SubjectSpecialtyFilterCB.ItemsSource = specialtiesData;
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
            var saveButton = (Control)sender;
            var dialog = Views[saveButton.Tag].Dialog;
            dialog.IsOpen = false;
            await NetworkUtils.ExecuteDataAction(saveButton.Tag.ToString(), this, (DBRecord)dialog.DataContext, DataAction.Save);
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DialogHost senderDialog) senderDialog.IsOpen = false;
            else Views[((Button)sender).Tag].Dialog.IsOpen = false;
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
            {
                await NetworkUtils.ExecuteDataAction(tag, null, current, DataAction.Remove);
                if (tag == "Group")
                    foreach (var currentStudent in (await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.Student),
                                                                                                  (nameof(Account.GroupID), current.ID))))
                    {
                        currentStudent.GroupID = -1;
                        await NetworkUtils.ExecuteDataAction<Account>(null, currentStudent, DataAction.Save);
                    }
                else if (tag == "Subject")
                    foreach (var currentTeacher in (await NetworkUtils.RequestData<Teacher>(this, (nameof(Teacher.SubjectID), current.ID))))
                        await NetworkUtils.ExecuteDataAction<Teacher>(null, currentTeacher, DataAction.Remove);
            }

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

        private void StudyObjectDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is DialogHost dialog)
                if (dialog.IsOpen)
                    if (e.Key == Key.Enter)
                    {
                        dialog.Focus();
                        SaveButton_Click(dialog, null);
                    }
                    else if (e.Key == Key.Escape)
                        CancelButton_Click(dialog, null);
        }

        private async void StudyObjectsWindow_KeyDown(object sender, KeyEventArgs e)
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

        void EnableFiltersCB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        void SemesterFilterTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        void SubjectSpecialtyFilterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            if (SubjectsLV != null)
                if (EnableFiltersCB.IsChecked ?? false)
                    SubjectsLV.Items.Filter = c =>
                    {
                        var subject = (Subject)c;
                        return (!int.TryParse(SemesterFilterTB.Text, out int semesterValue) || subject.Semester == -1 || subject.Semester == semesterValue) &&
                        (SubjectSpecialtyFilterCB.SelectedItem == null || SubjectSpecialtyFilterCB.SelectedItem == subject.Specialty);
                    };
                else SubjectsLV.Items.Filter = c => true;
        }
    }

    public class SemesterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == -1 ? "" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.TryParse(value.ToString(), out int intValue) ? intValue : -1;
        }
    }
}
