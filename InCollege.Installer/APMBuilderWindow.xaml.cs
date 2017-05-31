using Microsoft.Win32;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace InCollege.Installer
{
    public partial class APMBuilderWindow : Window
    {
        BinaryFormatter Formatter = new BinaryFormatter();

        public APMBuilderWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() ?? false)
                using (var stream = new FileStream(dialog.FileName, FileMode.Open))
                    DataContext = (AppPart)Formatter.Deserialize(stream);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            if (dialog.ShowDialog() ?? false)
                using (var stream = new FileStream(dialog.FileName, FileMode.Open))
                    Formatter.Serialize(stream, DataContext);
        }
    }
}
