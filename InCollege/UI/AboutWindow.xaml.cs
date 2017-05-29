using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InCollege.Client.UI
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            ShowInfo(InfoTag("Base"));
        }

        int Counter = 0;
        async void TitleTB_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (++Counter >= 10)
            {
                TitleTB.Foreground = Brushes.Red;
                TitleTB.IsEnabled = false;
                if (Counter > 20)
                {
                    Counter = 0;
                    MessageBox.Show("Замечательно! Вы нашли ее! Забирайте! ?assign=1");
                }
                else await Task.Run(() => Thread.Sleep(Counter == 10 ? 1000 : 300));
                TitleTB.Foreground = Brushes.Lime;
                TitleTB.IsEnabled = true;
            }
        }

        bool ComponentsMode = false;
        private void ComponentsButton_Click(object sender, RoutedEventArgs e)
        {
            ComponentsMode = !ComponentsMode;
            ShowInfo(InfoTag(ComponentsMode ? "Components" : "Base"));
            ComponentsButton.Content = ComponentsMode ? "Основная информация" : "Сторонние компоненты";
            ContactsButton.Visibility = ComponentsMode ? Visibility.Collapsed : Visibility.Visible;
        }

        bool ContactsMode = false;
        private void ContactsButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsMode = !ContactsMode;
            ShowInfo(InfoTag(ContactsMode ? "Contacts" : "Base"));
            ContactsButton.Content = ContactsMode ? "Основная информация" : "Контакты";
            ComponentsButton.Visibility = ContactsMode ? Visibility.Collapsed : Visibility.Visible;

        }

        void ShowInfo((string open, string close) tag)
        {
            AboutContentTB.Text = Properties.Resources.About
                                          .Split(new[] { tag.open }, StringSplitOptions.RemoveEmptyEntries)[1]
                                          .Split(new[] { tag.close }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        (string open, string close) InfoTag(string name)
        {
            return ($"[{name}]", $"[/{name}]");
        }
    }
}
