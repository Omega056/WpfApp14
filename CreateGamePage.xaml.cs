using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WpfApp14.Services;
using WpfApp14.UIModels;

namespace WpfApp14
{
    public partial class CreateGamePage : Page
    {
        public ObservableCollection<QuestionUIModel> Questions { get; set; } = new();

        public CreateGamePage()
        {
            InitializeComponent();
            DataContext = this;
            AdjustQuestions(5);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => NavigationService?.GoBack();

        private void QuestionsCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuestionsCountCombo.SelectedItem is ComboBoxItem item
                && int.TryParse(item.Content.ToString(), out int newCount))
                AdjustQuestions(newCount);
        }

        private void AdjustQuestions(int count)
        {
            while (Questions.Count < count)
                Questions.Add(new QuestionUIModel(Questions.Count + 1));
            while (Questions.Count > count)
                Questions.RemoveAt(Questions.Count - 1);
            for (int i = 0; i < Questions.Count; i++)
                Questions[i].UpdateHeader(i + 1);
        }

        private void AddQuizButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;
            MessageTextBlock.Text = string.Empty;

            string title = QuizTitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowError("Пожалуйста, введите название викторины.");
                return;
            }

            var validQuestions = Questions
                .Where(q => !string.IsNullOrWhiteSpace(q.QuestionText))
                .ToList();
            if (!validQuestions.Any())
            {
                ShowError("Нужно хотя бы один вопрос с текстом.");
                return;
            }

            var user = ((App)Application.Current).CurrentUser;
            if (user == null)
            {
                ShowError("Пользователь не аутентифицирован. Пожалуйста, войдите.");
                NavigationService?.Navigate(new LoginPage());
                return;
            }

            try
            {
                int quizId = DatabaseService.InsertQuiz(user.Id, title);
                ShowDebug($"Debug: Quiz inserted with ID={quizId}");

                int questionCount = 0;
                foreach (var q in validQuestions)
                {
                    var answers = q.Answers
                        .Where(a => !string.IsNullOrWhiteSpace(a.Text))
                        .Select(a => (a.Text, a.IsCorrect))
                        .ToArray();
                    if (answers.Length == 0)
                    {
                        ShowDebug($"Debug: Skipping question '{q.QuestionHeader}' (no valid answers)");
                        continue;
                    }
                    int timer = q.SelectedTimerIndex switch { 0 => 10, 1 => 20, 2 => 30, _ => 10 };
                    DatabaseService.InsertQuestion(quizId, q.QuestionText, timer, answers);
                    questionCount++;
                    ShowDebug($"Debug: Inserted question '{q.QuestionHeader}' with {answers.Length} answers");
                }

                if (questionCount == 0)
                {
                    ShowError("Не удалось сохранить вопросы. Добавьте хотя бы один ответ.");
                    DatabaseService.DeleteQuiz(quizId);
                    return;
                }
                ShowSuccess($"Викторина \"{title}\" сохранена (ID={quizId}, вопросов={questionCount}).");
                NavigationService?.Navigate(new FindGamePage());
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при сохранении викторины: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageTextBlock.Text = message;
            MessageTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            MessageTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowSuccess(string message)
        {
            MessageTextBlock.Text = message;
            MessageTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            MessageTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowDebug(string message)
        {
            MessageTextBlock.Text = message;
            MessageTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            MessageTextBlock.Visibility = Visibility.Visible;
        }
    }
}