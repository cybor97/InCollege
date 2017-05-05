using InCollege.Core.Data;
using System.Threading.Tasks;
using System.Windows;
using InCollege.Core.Data.Base;
using System.Windows.Input;
using InCollege.UI;

namespace InCollege.Client.UI.DictionariesUI
{
    public partial class DictionariesWindow : Window, IUpdatable
    {
        public DictionariesWindow()
        {
            InitializeComponent();
        }

        private async void DictionariesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            AttestationTypesLV.ItemsSource = await NetworkUtils.RequestData<AttestationType>(this);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AttestationTypeDialog.DataContext = new AttestationType();
            AttestationTypeDialog.IsOpen = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AttestationTypeDialog.IsOpen = false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await NetworkUtils.ExecuteDataAction<AttestationType>(this, (DBRecord)AttestationTypeDialog.DataContext, DataAction.Save);
            AttestationTypeDialog.IsOpen = false;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (AttestationTypesLV.SelectedItem != null)
            {
                AttestationTypeDialog.IsOpen = true;
                AttestationTypeDialog.DataContext = (AttestationType)AttestationTypesLV.SelectedItem;
            }
        }

        private async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (AttestationTypesLV.SelectedItems?.Count > 0)
                foreach (DBRecord current in AttestationTypesLV.SelectedItems)
                    await NetworkUtils.ExecuteDataAction<AttestationType>(null, current, DataAction.Remove);

            await UpdateData();
        }

        private void AttestationTypeDialog_KeyDown(object sender, KeyEventArgs e)
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
                    await UpdateData();
                    break;
            }
        }

        private async void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            await UpdateData();
        }

        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
