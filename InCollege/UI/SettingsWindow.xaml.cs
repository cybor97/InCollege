using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace InCollege.Client.UI
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = ClientConfiguration.Instance;
        }

        void PortTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(PortTB.Text + e.Text, "^\\d{1,4}$");
        }

        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ClientConfiguration.Instance.Save();
            Close();
        }
    }
}
