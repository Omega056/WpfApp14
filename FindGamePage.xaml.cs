using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class FindGamePage : Page
    {
        private List<QuizInfo> _quizzes = new();

        public FindGamePage()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            _quizzes = DatabaseService.GetAllQuizzes();
            QuizzesListView.ItemsSource = _quizzes;
        }

        private void QuizzesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Чтобы выбрать для игры, не для удаления
            // Список просто обновляется при клике по строке
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void DeleteQuizButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuizzesListView.SelectedItem is not QuizInfo qi)
            {
                MessageBox.Show("Пожалуйста, выберите викторину в списке.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить викторину \"{qi.Title}\" (ID={qi.Id})?",
                "Подтвердите удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                DatabaseService.DeleteQuiz(qi.Id);
                MessageBox.Show($"Викторина \"{qi.Title}\" удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadQuizzes();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Не удалось удалить викторину:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
