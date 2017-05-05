using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using InCollege.UI.Util;
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

            var departmentHeads = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead));
            DepartmentHeadCB.ItemsSource = departmentHeads;

            var departmentsData = await NetworkUtils.RequestData<Department>(this);
            foreach (Department current in departmentsData)
                current.DepartmentHead = departmentHeads.FirstOrDefault(c => c.ID == current.DepartmentHeadID);
            DepartmentsLV.ItemsSource = departmentsData;

            SpecialtiesLV.ItemsSource = await NetworkUtils.RequestData<Specialty>(this);
            foreach (Specialty current in SpecialtiesLV.ItemsSource)
                current.DepartmentName = (await NetworkUtils.RequestData<Department>(this, (nameof(Department.ID), current.DepartmentID)))?.FirstOrDefault()?.DepartmentName;

            GroupsLV.ItemsSource = await NetworkUtils.RequestData<Group>(this);
            foreach (Group current in GroupsLV.ItemsSource)
                current.SpecialtyName = (await NetworkUtils.RequestData<Specialty>(this, (nameof(Specialty.ID), current.SpecialtyID)))?.FirstOrDefault()?.SpecialtyName;

            SubjectsLV.ItemsSource = await NetworkUtils.RequestData<Subject>(this);
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
            Button saveButton = (Button)sender;
            var dialog = Views[saveButton.Tag].Dialog;
            dialog.IsOpen = false;
            switch (saveButton.Tag.ToString())
            {
                case "Department":
                    var record = (Department)dialog.DataContext;
                    record.DepartmentHeadID = ((DBRecord)DepartmentHeadCB?.SelectedItem)?.ID ?? -1;
                    await NetworkUtils.ExecuteDataAction<Department>(this, record, DataAction.Save);
                    break;
                case "Specialty":
                    await NetworkUtils.ExecuteDataAction<Specialty>(this, (DBRecord)dialog.DataContext, DataAction.Save);
                    break;
                case "Group":
                    await NetworkUtils.ExecuteDataAction<Group>(this, (DBRecord)dialog.DataContext, DataAction.Save);
                    break;
                case "Subject":
                    await NetworkUtils.ExecuteDataAction<Subject>(this, (DBRecord)dialog.DataContext, DataAction.Save);
                    break;
            }
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Views[((Button)sender).Tag].Dialog.IsOpen = false;
        }

        private void Something_SelectionChanged_Ignore(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            var view = Views[((TabItem)StudyObjectsTabs.SelectedItem).Tag];
            var dialog = view.Dialog;
            dialog.DataContext = view.ListView.SelectedItem;
            dialog.IsOpen = true;
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var item = UIUtils.FindAncestorOrSelf<ListView>(sender as MenuItem);
            MessageBox.Show(item.ToString());

        }
    }
}
