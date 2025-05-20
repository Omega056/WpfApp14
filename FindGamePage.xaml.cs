// FindGamePage.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp14.Services;
using static WpfApp14.Services.DatabaseService;

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

        // Запуск игры двойным кликом
        private void QuizzesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (QuizzesListView.SelectedItem is QuizInfo quiz)
                NavigateToGame(quiz.Id);
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuizzesListView.SelectedItem is not QuizInfo quiz)
            {
                MessageBox.Show("Пожалуйста, выберите викторину.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NavigateToGame(quiz.Id);
        }

        private void NavigateToGame(int quizId)
        {
            if (NavigationService == null)
            {
                MessageBox.Show("Навигация недоступна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            NavigationService.Navigate(new GamePage(quizId));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
