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
            AboutContentTB.Text = Properties.Resources.About;
        }

        int Counter = 0;
        async void TitleTB_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (++Counter == 10)
            {
                TitleTB.Foreground = Brushes.Red;
                TitleTB.IsEnabled = false;
                await Task.Run(() => Thread.Sleep(1000));
                if (Counter > 15)
                {
                    Counter = 0;
                    MessageBox.Show("Замечательно! Вы нашли ее! Забирайте! ?assign=1");
                }
                TitleTB.Foreground = Brushes.Lime;
                TitleTB.IsEnabled = true;
            }
        }
    }
}
