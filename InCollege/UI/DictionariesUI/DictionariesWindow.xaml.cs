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

        public bool AddMode = false;

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
            AddMode = true;
            TypeNameTB.Text = null;
            AddAttestationTypeDialog.IsOpen = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAttestationTypeDialog.IsOpen = false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var attestationType = AddMode ? new AttestationType { TypeName = TypeNameTB.Text } : (AttestationType)AttestationTypesLV.SelectedItem;
            attestationType.TypeName = TypeNameTB.Text;
            attestationType.Modified = true;
            await NetworkUtils.ExecuteDataAction<AttestationType>(this, attestationType, DataAction.Save);

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
                foreach (DBRecord current in AttestationTypesLV.SelectedItems)
                    updateRequired = updateRequired ||
                         (int.TryParse(await NetworkUtils.ExecuteDataAction<AttestationType>(null, current, DataAction.Remove), out int newID) &&
                         newID > -1);

            if (updateRequired)
                await UpdateData();
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
