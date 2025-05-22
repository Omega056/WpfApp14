using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp14.Services;
using static WpfApp14.Services.DatabaseService;

namespace WpfApp14
{
    public partial class GamePage : Page
    {
        private readonly List<QuestionDataModel> _questions;
        private int _currentIndex;
        private readonly DispatcherTimer _timer;
        private int _remainingSeconds;

        public GamePage(int quizId)
        {
            InitializeComponent();

            _questions = DatabaseService.GetQuestionsForQuiz(quizId) ?? new List<QuestionDataModel>();

            if (_questions.Count == 0)
            {
                MessageBox.Show("Нет вопросов для этой викторины.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService?.GoBack();
                return;
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _currentIndex = 0;
            ShowNextQuestion();
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            TimerTextBlock.Text = _remainingSeconds.ToString();
            if (_remainingSeconds <= 0)
            {
                _timer.Stop();
                ShowNextQuestion();
            }
        }

        private void ShowNextQuestion()
        {
            if (_currentIndex >= _questions.Count)
            {
                MessageBox.Show("Викторина завершена!", "Результаты", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
                return;
            }

            var question = _questions[_currentIndex];
            QuestionTextBlock.Text = question.Text;
            _remainingSeconds = question.TimerSeconds;
            TimerTextBlock.Text = _remainingSeconds.ToString();

            var buttons = new[] { AnswerButton1, AnswerButton2, AnswerButton3, AnswerButton4 };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < question.Answers.Count)
                {
                    buttons[i].Visibility = Visibility.Visible;
                    buttons[i].Content = question.Answers[i].Text;
                    buttons[i].Tag = question.Answers[i].IsCorrect;
                    buttons[i].IsEnabled = true;
                    buttons[i].Background = System.Windows.Media.Brushes.White;
                }
                else
                {
                    buttons[i].Visibility = Visibility.Collapsed;
                }
            }
            _timer.Start();
        }

        private void OnAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is bool isCorrect)
            {
                _timer.Stop();
                MarkAnswers();
                _currentIndex++;

                var delay = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                delay.Tick += (_, _) =>
                {
                    delay.Stop();
                    ShowNextQuestion();
                };
                delay.Start();
            }
        }

        private void MarkAnswers()
        {
            var buttons = new[] { AnswerButton1, AnswerButton2, AnswerButton3, AnswerButton4 };
            foreach (var btn in buttons.Where(b => b.Visibility == Visibility.Visible))
            {
                bool correct = btn.Tag is bool c && c;
                btn.Background = correct ? System.Windows.Media.Brushes.LightGreen : System.Windows.Media.Brushes.LightCoral;
                btn.IsEnabled = false;
            }
        }

        private void OnQuit_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            NavigationService?.GoBack();
        }
    }
}
