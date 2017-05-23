using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace InCollege.Installer
{
    public partial class MainWindow : Window
    {
        ResourceSet ResourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        List<AppPart> InstallComponents = new List<AppPart>();
        List<string> ResourceKeys = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        async void LogoAnimation_Completed(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                foreach (DictionaryEntry entry in ResourceSet)
                    if (entry.Key.ToString().EndsWith("APM"))
                        InstallComponents.Add((AppPart)new BinaryFormatter().Deserialize(new MemoryStream((byte[])entry.Value)));
            });

            ComponentsLV.ItemsSource = InstallComponents;

            PreInstallTB.Visibility = Visibility.Collapsed;
            ((Storyboard)InstallButtons.Resources["InstallButtonsAppearStoryboard"]).Begin();
            ((Storyboard)ComponentsLV.Resources["ComponentsLVAppearStoryboard"]).Begin();
        }

        void SelectedCB_CheckChanged(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = InstallComponents.Any(c => c.Selected);
        }

        async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            PreInstallTB.Visibility = Visibility.Visible;
            PreInstallTB.Text = "Идет установка ПО...";

            InstallButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            ((Storyboard)InstallButtons.Resources["InstallButtonsDisappearStoryboard"]).Begin();
            ((Storyboard)ComponentsLV.Resources["ComponentsLVDisappearStoryboard"]).Begin();


            foreach (var currentComponent in InstallComponents)
                if (currentComponent.Selected)
                {
                    PreInstallTB.Text = $"Установка {currentComponent.DisplayName}...";
                    await Task.Run(() =>
                    {
                        currentComponent.InstallLocation = currentComponent.InstallLocation
                            .Replace("INSTALL_ROOT", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IN_COLLEGE_INSTALLER_TEST"));

                        currentComponent.ExecutablePath = currentComponent.ExecutablePath
                            .Replace("INSTALL_LOCATION", currentComponent.InstallLocation);

                        currentComponent.DisplayIcon = currentComponent.DisplayIcon
                            .Replace("INSTALL_LOCATION", currentComponent.InstallLocation);

                        currentComponent.UninstallString = currentComponent.UninstallString
                            .Replace("INSTALL_LOCATION", currentComponent.InstallLocation);

                        if (!Directory.Exists(CommonVariables.TMPDirPath)) Directory.CreateDirectory(CommonVariables.TMPDirPath);
                        var tmpFilename = Path.Combine(CommonVariables.TMPDirPath, currentComponent.ID);
                        File.WriteAllBytes(tmpFilename, (byte[])ResourceSet.GetObject(currentComponent.ID.Replace('.', '_')));

                        if (!Directory.Exists(currentComponent.InstallLocation)) Directory.CreateDirectory(currentComponent.InstallLocation);
                        var zip = ZipStorer.Open(tmpFilename, FileAccess.Read);
                        foreach (var current in zip.ReadCentralDir())
                            if (!current.FilenameInZip.EndsWith("\\") && !current.FilenameInZip.EndsWith("/"))
                                zip.ExtractFile(current, Path.Combine(currentComponent.InstallLocation, current.FilenameInZip));

                        File.Copy(Assembly.GetExecutingAssembly().Location, currentComponent.UninstallString, true);

                        RegistryManager.CreateApplicationEntry(currentComponent);
                        LNKManager.CreateLinks(currentComponent, Path.Combine(CommonVariables.DesktopPath, $"{currentComponent.DisplayName}.lnk"));
                    });
                }

            PreInstallTB.Text = "Установка успешно завершена!";
            await Task.Run(() => Thread.Sleep(2000));
            Close();
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
