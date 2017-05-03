using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InCollege.UI
{
    public partial class StudyObjectsWindow : Window
    {
        public StudyObjectsWindow(int tabIndex)
        {
            InitializeComponent();
            StudyObjectsTabs.SelectedIndex = tabIndex;
        }

        async Task UpdateDisplayData()
        {
            Title = ((TabItem)StudyObjectsTabs.SelectedItem).Header.ToString();

            //try
            //{
            //    StatementsLV.Items.Clear();
            //    if ((response = (await new HttpClient()
            //          .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
            //          new StringContent(
            //          $"Action=GetRange&" +
            //          $"table={nameof(Statement)}&" +
            //          $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
            //    {
            //        var result = JsonConvert.DeserializeObject<IEnumerable<Statement>>(await response.Content.ReadAsStringAsync());
            //        //Yeah, 2 times, to prevent multiplying
            //        StatementsLV.Items.Clear();
            //        foreach (var current in result)
            //            StatementsLV.Items.Add(current);
            //    }
            //    else
            //        MessageBox.Show(await response.Content.ReadAsStringAsync());
            //}
            //catch (HttpRequestException exc)
            //{
            //    MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            //}

        }

        async void AccountTypesTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateDisplayData();
        }

        async void StudyObjectsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            switch (StudyObjectsTabs.SelectedIndex)
            {
                case 0:
                    EditDepartmentDialog.IsOpen = true;
                    break;
                case 1:
                    EditSpecialtyDialog.IsOpen = true;
                    break;
                case 2:
                    EditGroupDialog.IsOpen = true;
                    break;
                case 3:
                    EditSubjectDialog.IsOpen = true;
                    break;
            }
        }

        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Tag.ToString())
            {
                case "Department":
                    EditDepartmentDialog.IsOpen = true;
                    break;
                case "Specialty":
                    EditSpecialtyDialog.IsOpen = true;
                    break;
                case "Group":
                    EditGroupDialog.IsOpen = true;
                    break;
                case "Subject":
                    EditSubjectDialog.IsOpen = true;
                    break;
            }
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Tag.ToString())
            {
                case "Department":
                    EditDepartmentDialog.IsOpen = false;
                    break;
                case "Specialty":
                    EditSpecialtyDialog.IsOpen = false;
                    break;
                case "Group":
                    EditGroupDialog.IsOpen = false;
                    break;
                case "Subject":
                    EditSubjectDialog.IsOpen = false;
                    break;
            }
        }
    }
}
