using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class ProfilePage : Page
    {
        private bool _isEditingUsername;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            var profile = UserProfile.Current;
            UsernameTextBlock.Text = profile.Username;
            IQTextBlock.Text = profile.IQ.ToString();
            CorrectAnswersTextBlock.Text = $"{profile.CorrectAnswerPercentage}%";
            UsernameTextBox.Text = profile.Username;
        }

        private void EditUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditingUsername)
            {
                // Переключение в режим редактирования
                UsernameTextBlock.Visibility = Visibility.Collapsed;
                UsernameTextBox.Visibility = Visibility.Visible;
                EditUsernameButton.Content = "Сохранить";
                _isEditingUsername = true;
            }
            else
            {
                // Сохранение имени
                string newUsername = UsernameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(newUsername))
                {
                    MessageBox.Show("Имя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UserProfile.Current.Username = newUsername;
                UsernameTextBlock.Text = newUsername;
                UsernameTextBlock.Visibility = Visibility.Visible;
                UsernameTextBox.Visibility = Visibility.Collapsed;
                EditUsernameButton.Content = "Изменить имя";
                _isEditingUsername = false;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Для примера просто возвращаемся на главную страницу
            // В реальном приложении здесь может быть логика выхода из аккаунта
            NavigationService?.Navigate(new HomePage());
        }
    }
}