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
    public partial class StudyObjectsWindow : Window
    {
        Dictionary<object, DialogHost> Dialogs;

        public StudyObjectsWindow(int tabIndex)
        {
            InitializeComponent();
            StudyObjectsTabs.SelectedIndex = tabIndex;

            Dialogs = new Dictionary<object, DialogHost>
            {
                { "Department", EditDepartmentDialog },
                { "Specialty", EditSpecialtyDialog },
                { "Group", EditGroupDialog },
                { "Subject", EditSubjectDialog }
            };
        }

        async Task UpdateDisplayData()
        {
            Title = ((TabItem)StudyObjectsTabs.SelectedItem).Header.ToString();

            DepartmentHeadCB.ItemsSource = await NetworkUtils.RequestData<Account>(this, (nameof(Account.AccountType), AccountType.DepartmentHead));

            DepartmentsLV.ItemsSource = await NetworkUtils.RequestData<Department>(this);
            foreach (Department current in DepartmentsLV.ItemsSource)
                current.DepartmentHeadName = (await NetworkUtils.RequestData<Account>(this, (nameof(Account.ID), current.DepartmentHeadID)))?.FirstOrDefault()?.FullName;

            SpecialtiesLV.ItemsSource = await NetworkUtils.RequestData<Specialty>(this);
            GroupsLV.ItemsSource = await NetworkUtils.RequestData<Group>(this);
            SubjectsLV.ItemsSource = await NetworkUtils.RequestData<Subject>(this);
        }

        async void StudyObjectsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateDisplayData();
        }

        async void StudyObjectsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Dialogs[((TabItem)StudyObjectsTabs.SelectedItem).Tag];
            dialog.IsOpen = true;
            dialog.DataContext =
                dialog == EditDepartmentDialog ? new Department() :
                dialog == EditSpecialtyDialog ? new Specialty() :
                dialog == EditGroupDialog ? new Group() :
                dialog == EditSubjectDialog ? (DBRecord)new Subject() :
                null;
        }

        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs[((Button)sender).Tag].IsOpen = false;
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs[((Button)sender).Tag].IsOpen = false;
        }
    }
}
