using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            // Debug: Confirm page initialization
            MessageBox.Show("LoginPage initialized", "DEBUG");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug: Confirm button click
            MessageBox.Show("LoginButton_Click triggered", "DEBUG");

            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите имя пользователя и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = DatabaseService.AuthenticateUser(username, password);
                if (user != null)
                {
                    ((App)Application.Current).CurrentUser = user;
                    if (NavigationService == null)
                    {
                        MessageBox.Show("NavigationService is null", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    NavigationService.Navigate(new HomePage());
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug: Confirm button click
            MessageBox.Show("RegisterButton_Click triggered", "DEBUG");

            if (NavigationService == null)
            {
                MessageBox.Show("NavigationService is null", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            NavigationService.Navigate(new RegisterPage());
        }
    }
}