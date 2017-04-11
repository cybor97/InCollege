using System.Windows;

namespace InCollege.Client.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DictionariesItem_Click(object sender, RoutedEventArgs e)
        {
            new DictionariesWindow().ShowDialog();
        }

        private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
    }
}
