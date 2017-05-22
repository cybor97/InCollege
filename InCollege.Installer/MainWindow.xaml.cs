using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media.Animation;

namespace InCollege.Installer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ComponentsLV.ItemsSource = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.apm")
                                                .Select(c => new BinaryFormatter().Deserialize(new FileStream(c, FileMode.Open)));
        }

        void LogoAnimation_Completed(object sender, EventArgs e)
        {
            ((Storyboard)ComponentsLV.Resources["ComponentsLVAppearStoryboard"]).Begin();
        }

        void SelectedCB_CheckChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
