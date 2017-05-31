using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        string MyFileName = Assembly.GetExecutingAssembly().Location;
        string CurrentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        bool UninstallerMode = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        async void LogoAnimation_Completed(object sender, EventArgs e)
        {
            try
            {
                if (Environment.CommandLine?.Contains("-generate-apm") ?? false)
                {
                    new APMBuilderWindow().ShowDialog();
                    Close();
                }

                await Task.Run(() =>
                {
                    foreach (DictionaryEntry entry in ResourceSet)
                        if (entry.Key.ToString().EndsWith("APM"))
                            InstallComponents.Add((AppPart)new BinaryFormatter().Deserialize(new MemoryStream((byte[])entry.Value)));
                });

                UninstallerMode = InstallComponents.Any(c => MyFileName.ToLower().Contains(c.UninstallString.Replace("INSTALL_LOCATION", string.Empty).ToLower()));

                PreInstallTB.Visibility = Visibility.Collapsed;
                ((Storyboard)InstallButtons.Resources["InstallButtonsAppearStoryboard"]).Begin();

                if (UninstallerMode)
                {
                    InstallButton.Content = "Удалить";
                    InstallButton.IsEnabled = true;
                }
                else
                {
                    ComponentsLV.ItemsSource = InstallComponents;
                    ((Storyboard)ComponentsLV.Resources["ComponentsLVAppearStoryboard"]).Begin();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка!\n\n{exc}");
            }
        }

        void SelectedCB_CheckChanged(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = InstallComponents.Any(c => c.Selected);
        }

        async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UninstallerMode)
                    await UninstallApp();
                else
                    await InstallApp(CommonVariables.InstallRoot);

                Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка!\n\n{exc}");
            }
        }

        async Task UninstallApp()
        {
            PreInstallTB.Visibility = Visibility.Visible;
            PreInstallTB.Text = "Подготовка к удалению...\nЗакройте все окна приложения";

            await Task.Run(() =>
            {
                var myID = Process.GetCurrentProcess().Id;
                while (Process.GetProcesses().Any(c => c.ProcessName.ToLower().Contains("incollege") && c.Id != myID))
                    Thread.Sleep(100);
            });

            foreach (var currentComponent in InstallComponents.Select(c => RegistryManager.GetInstalledEntryInfo(c.ID)))
                if (currentComponent != null)
                {
                    var files = Directory.GetFiles(CurrentDirectory).Where(c => c.EndsWith("APM")).ToArray();
                    if (files.Length > 0)
                    {
                        AppPart localComponent = null;
                        await Task.Run(() =>
                        {
                            while (true)
                                try
                                {
                                    using (var stream = new FileStream(files[0], FileMode.Open))
                                    {
                                        localComponent = ((AppPart)new BinaryFormatter().Deserialize(stream));
                                        stream.Close();
                                    }
                                    break;
                                }
                                catch (IOException)
                                {
                                    Thread.Sleep(100);
                                }
                        });
                        if (currentComponent.ID == localComponent.ID)
                        {
                            PreInstallTB.Text = $"Удаление \"{currentComponent.DisplayName}\"...";
                            await Task.Run(() =>
                            {
                                foreach (var current in Directory.GetFiles(currentComponent.InstallLocation))
                                    if (current != MyFileName)
                                        try
                                        {
                                            File.Delete(current);
                                        }
                                        catch (IOException) { }

                                RegistryManager.RemoveApplicationEntry(currentComponent.ID);
                                LNKManager.RemoveLinks(Path.Combine(CommonVariables.DesktopPath, $"{currentComponent.DisplayName}.lnk"));
                                Process.Start(new ProcessStartInfo("cmd", $" /C timeout 10&&rd /s /q \"{CurrentDirectory}\"") { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
                            });
                            break;
                        }
                    }
                }
            PreInstallTB.Text = "Удаление успешно завершено!";
            await Task.Run(() => Thread.Sleep(2000));
            Process.GetCurrentProcess().Kill();
        }

        async Task InstallApp(string installRoot)
        {
            PreInstallTB.Visibility = Visibility.Visible;
            PreInstallTB.Text = "Идет установка ПО...";

            InstallButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            ((Storyboard)InstallButtons.Resources["InstallButtonsDisappearStoryboard"]).Begin();
            ((Storyboard)ComponentsLV.Resources["ComponentsLVDisappearStoryboard"]).Begin();

            if (!Directory.Exists(CommonVariables.TMPDirPath)) Directory.CreateDirectory(CommonVariables.TMPDirPath);
            foreach (var currentComponent in InstallComponents)
                if (currentComponent.Selected)
                {
                    PreInstallTB.Text = $"Установка \"{currentComponent.DisplayName}\"...";
                    await Task.Run(() =>
                    {
                        currentComponent.InstallLocation = currentComponent.InstallLocation
                            .Replace("INSTALL_ROOT", installRoot);

                        currentComponent.ExecutablePath = currentComponent.ExecutablePath
                            .Replace("INSTALL_LOCATION", currentComponent.InstallLocation);

                        currentComponent.DisplayIcon = currentComponent.DisplayIcon
                            .Replace("INSTALL_LOCATION", currentComponent.InstallLocation);

                        currentComponent.UninstallString = currentComponent.UninstallString
                            .Replace("INSTALL_LOCATION", currentComponent.InstallLocation);

                        var tmpFilename = Path.Combine(CommonVariables.TMPDirPath, currentComponent.ID);
                        File.WriteAllBytes(tmpFilename, (byte[])ResourceSet.GetObject(currentComponent.ID.Replace('.', '_')));

                        if (!Directory.Exists(currentComponent.InstallLocation)) Directory.CreateDirectory(currentComponent.InstallLocation);
                        var zip = ZipStorer.Open(tmpFilename, FileAccess.Read);
                        foreach (var current in zip.ReadCentralDir())
                            if (!current.FilenameInZip.EndsWith("\\") && !current.FilenameInZip.EndsWith("/"))
                                zip.ExtractFile(current, Path.Combine(currentComponent.InstallLocation, current.FilenameInZip));
                        zip.Close();

                        var componentMarkupResourceName = $"{currentComponent.ID.Replace('.', '_')}_APM";
                        File.WriteAllBytes(Path.Combine(Directory.GetParent(currentComponent.UninstallString).FullName, componentMarkupResourceName),
                            (byte[])((DictionaryEntry)ResourceSet.OfType<object>().FirstOrDefault(c => ((DictionaryEntry)c).Key.ToString() == componentMarkupResourceName)).Value);
                        File.Copy(MyFileName, currentComponent.UninstallString, true);

                        RegistryManager.CreateApplicationEntry(currentComponent);
                        LNKManager.CreateLinks(currentComponent, Path.Combine(CommonVariables.DesktopPath, $"{currentComponent.DisplayName}.lnk"));
                    });
                }

            PreInstallTB.Text = "Удаление временных файлов...";
            await Task.Run(() =>
            {
                while (true)
                    try
                    {
                        Directory.Delete(CommonVariables.TMPDirPath, true);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                    }
            });

            PreInstallTB.Text = "Установка успешно завершена!";
            await Task.Run(() => Thread.Sleep(2000));
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
