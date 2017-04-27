using System;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Animation;

namespace InCollege.Client.UI.AuthorizationUI
{
    public partial class AuthorizationWindow : Window
    {
        bool _SignUpMode;
        bool SignUpMode
        {
            get
            {
                return _SignUpMode;
            }

            set
            {
                SignUpElementsVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                var storyboard = (Storyboard)Resources["SwitchModeStoryboard"];
                ((DoubleAnimation)storyboard.Children[0]).To = value ? 350 : 200;
                storyboard.Begin();
                LoginButton.Content = value ? "Зарегистрироваться" : "Войти";
                ChangeModeButton.Content = value ? "Войти" : "Зарегистироваться";
                _SignUpMode = value;
            }
        }

        public static DependencyProperty SignUpElementsVisibilityProperty = DependencyProperty.Register("SignUpElementsVisibility", typeof(Visibility), typeof(AuthorizationWindow));

        Visibility SignUpElementsVisibility
        {
            get
            {
                return (Visibility)GetValue(SignUpElementsVisibilityProperty);
            }
            set
            {
                SetValue(SignUpElementsVisibilityProperty, value);
            }
        }

        public AuthorizationWindow()
        {
            InitializeComponent();
            SignUpMode = false;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.Visibility = ChangeModeButton.Visibility = Visibility.Collapsed;
            LoginProgressBar.Visibility = Visibility.Visible;
            HttpResponseMessage response;
            if (!SignUpMode || BirthdateTB.SelectedDate.HasValue)
                try
                {
                    if ((response = (await new HttpClient()
                          .PostAsync($"http://{ClientConfiguration.HostName}:{ClientConfiguration.Port}/Auth",
                          new StringContent(
                          $"Action={(SignUpMode ? "SignUp" : "SignIn")}&" +
                          (SignUpMode ? $"FullName={FullNameTB.Text}&" : string.Empty) +
                          $"UserName={UserNameTB.Text}&" +
                          $"Password={PasswordTB.Password}&" +
                          (SignUpMode ? $"BirthDate={Uri.EscapeDataString(BirthdateTB.SelectedDate.Value.ToString("MM\\/dd\\/yyyy"))}&" : string.Empty) +
                          (SignUpMode ? $"AccountType={AccountTypeCB.SelectedIndex}" : string.Empty))))).StatusCode == HttpStatusCode.OK)
                    {
                        App.Token = await response.Content.ReadAsStringAsync();
                        //TODO:Implement token storing for later sessions.
                        DialogResult = true;
                        Close();
                    }
                    else
                        MessageBox.Show(await response.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException exc)
                {
                    MessageBox.Show($"Ошибка подключения к серверу. Проверьте:\n-запущен ли сервер\n-настройки брандмауэра\n-правильно ли указан адрес\nТехническая информация:\n\n{exc.Message}");
                }


            LoginButton.Visibility = ChangeModeButton.Visibility = Visibility.Visible;
            LoginProgressBar.Visibility = Visibility.Collapsed;
        }

        private void ChangeModeButton_Click(object sender, RoutedEventArgs e)
        {
            SignUpMode = !SignUpMode;
        }

        private void _this_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                LoginButton_Click(null, null);
        }

        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SysadminModeItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Не верю!");
        }
    }
}
