using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp14.Services;
using static WpfApp14.Services.DatabaseService;

namespace WpfApp14
{
    public partial class FindGamePage : Page
    {
        public FindGamePage()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;
            MessageTextBlock.Text = string.Empty;

            try
            {
                var user = ((App)Application.Current).CurrentUser;
                if (user == null)
                {
                    ShowError("Пользователь не аутентифицирован. Пожалуйста, войдите.");
                    NavigationService?.Navigate(new LoginPage());
                    return;
                }

                var quizzes = DatabaseService.GetAllQuizzes(user.Id);
                QuizzesListView.ItemsSource = quizzes;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при загрузке викторин: {ex.Message}");
                NavigationService?.Navigate(new HomePage());
            }
        }

        private void QuizzesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (QuizzesListView.SelectedItem is QuizInfo quiz)
                NavigateToGame(quiz.Id);
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;
            MessageTextBlock.Text = string.Empty;

            if (QuizzesListView.SelectedItem is not QuizInfo quiz)
            {
                ShowError("Пожалуйста, выберите викторину.");
                return;
            }
            NavigateToGame(quiz.Id);
        }

        private void DeleteQuizButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;
            MessageTextBlock.Text = string.Empty;

            if (QuizzesListView.SelectedItem is not QuizInfo quiz)
            {
                ShowError("Пожалуйста, выберите викторину для удаления.");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить викторину '{quiz.Title}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseService.DeleteQuiz(quiz.Id);
                    LoadQuizzes();
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка при удалении викторины: {ex.Message}");
                }
            }
        }

        private void NavigateToGame(int quizId)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;
            MessageTextBlock.Text = string.Empty;

            if (NavigationService == null)
            {
                ShowError("Навигация недоступна.");
                return;
            }

            try
            {
                var questions = DatabaseService.GetQuestionsForQuiz(quizId);
                if (questions == null || questions.Count == 0)
                {
                    ShowError("В выбранной викторине нет вопросов.");
                    return;
                }

                NavigationService.Navigate(new GamePage(questions));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при переходе к игре: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());
        }

        private void ShowError(string message)
        {
            MessageTextBlock.Text = message;
            MessageTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            MessageTextBlock.Visibility = Visibility.Visible;
        }
    }
}