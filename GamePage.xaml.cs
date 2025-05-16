using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class GamePage : Page
    {
        private readonly int _quizId;
        private readonly System.Collections.Generic.List<QuestionDataModel> _questions;
        // ...
        public GamePage(int quizId)
        {
            InitializeComponent();
            _quizId = quizId;
            _questions = DatabaseService.GetQuestionsForQuiz(_quizId);
            StartQuestion();
        }

        private void StartQuestion()
        {
            throw new NotImplementedException();
        }
    }
}