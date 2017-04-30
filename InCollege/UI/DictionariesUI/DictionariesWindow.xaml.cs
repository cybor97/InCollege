using InCollege.Core.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using InCollege.Core.Data.Base;
using System.Windows.Input;

namespace InCollege.Client.UI.DictionariesUI
{
    public partial class DictionariesWindow : Window
    {
        public DictionariesWindow()
        {
            InitializeComponent();
        }

        public bool AddMode = false;

        private async void DictionariesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        async Task UpdateDisplayData()
        {
            try
            {
                AttestationTypesLV.Items.Clear();
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=GetRange&" +
                      $"table={nameof(AttestationType)}&" +
                      $"token={App.Token}")))).StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<IEnumerable<AttestationType>>(await response.Content.ReadAsStringAsync());
                    //Yeah, 2 times, to prevent multiplying
                    AttestationTypesLV.Items.Clear();
                    foreach (var current in result)
                        AttestationTypesLV.Items.Add(current);
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
                    if (response.StatusCode == HttpStatusCode.Forbidden) Close();
                }
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddMode = true;
            AddAttestationTypeDialog.IsOpen = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAttestationTypeDialog.IsOpen = false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpResponseMessage response;
                if ((response = (await new HttpClient()
                      .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                      new StringContent(
                      $"Action=Save&" +
                      $"table={nameof(AttestationType)}&" +
                      $"token={App.Token}&" +
                      (AttestationTypesLV.SelectedItem != null ? $"fieldID={((DBRecord)AttestationTypesLV.SelectedItem).ID}&" : $"") +
                      $"fieldTypeName={TypeNameTB.Text}&" +
                      $"fieldIsLocal={(AddMode ? 1 : 0)}&" +
                      $"fieldModified=1")))).StatusCode == HttpStatusCode.OK)
                    await UpdateDisplayData();
                else
                    MessageBox.Show(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException exc)
            {
                MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
            }

            AddAttestationTypeDialog.IsOpen = false;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (AttestationTypesLV.SelectedIndex != -1 && AttestationTypesLV.SelectedItems.Count == 1)
            {
                AddAttestationTypeDialog.IsOpen = true;
                TypeNameTB.Text = ((AttestationType)AttestationTypesLV.SelectedItem).TypeName;
                AddMode = false;
            }
        }

        private async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            bool updateRequired = false;
            if (AttestationTypesLV.SelectedItems?.Count > 0)
                try
                {
                    foreach (DBRecord current in AttestationTypesLV.SelectedItems)
                    {
                        HttpResponseMessage response;
                        if ((response = (await new HttpClient()
                              .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Data",
                              new StringContent(
                              $"Action=Remove&" +
                              $"token={App.Token}&" +
                              $"table={nameof(AttestationType)}&" +
                              $"id={current.ID}")))).StatusCode == HttpStatusCode.OK)
                            updateRequired = updateRequired || response.StatusCode == HttpStatusCode.OK;
                        else
                            MessageBox.Show(await response.Content.ReadAsStringAsync());
                    }
                }
                catch (HttpRequestException exc)
                {
                    MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
                }

            if (updateRequired)
                await UpdateDisplayData();
        }

        private void AddAttestationTypeDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SaveButton_Click(null, null);
            else if (e.Key == Key.Escape)
                CancelButton_Click(null, null);
        }

        private async void DictionariesWindow_KeyDown(object sender, KeyEventArgs e)
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
                    await UpdateDisplayData();
                    break;
            }
        }

        private async void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            await UpdateDisplayData();
        }

        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
