using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            // Debug: Confirm page initialization
            MessageBox.Show("RegisterPage initialized", "DEBUG");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug: Confirm button click
            MessageBox.Show("RegisterButton_Click triggered", "DEBUG");

            string username = UsernameTextBox.Text.Trim();
            string password = ShowPasswordCheckBox.IsChecked == true ? PasswordTextBox.Text : PasswordBox.Password;
            string repeatPassword = ShowPasswordCheckBox.IsChecked == true ? RepeatPasswordTextBox.Text : RepeatPasswordBox.Password;

            // Validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (username.Length < 3)
            {
                MessageBox.Show("Имя пользователя должно содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != repeatPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int userId = DatabaseService.RegisterUser(username, password);
                var user = DatabaseService.AuthenticateUser(username, password);
                if (user != null)
                {
                    ((App)Application.Current).CurrentUser = user;
                    if (NavigationService == null)
                    {
                        MessageBox.Show("NavigationService is null", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new HomePage());
                }
                else
                {
                    MessageBox.Show("Ошибка при регистрации. Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint failed
            {
                MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;

            RepeatPasswordTextBox.Text = RepeatPasswordBox.Password;
            RepeatPasswordBox.Visibility = Visibility.Collapsed;
            RepeatPasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;

            RepeatPasswordBox.Password = RepeatPasswordTextBox.Text;
            RepeatPasswordTextBox.Visibility = Visibility.Collapsed;
            RepeatPasswordBox.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug: Confirm button click
            MessageBox.Show("BackButton_Click triggered", "DEBUG");

            if (NavigationService == null)
            {
                MessageBox.Show("NavigationService is null", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            NavigationService.Navigate(new LoginPage());
        }
    }
}