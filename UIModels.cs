using System.Collections.ObjectModel;

namespace WpfApp14.UIModels
{
    public class QuestionUIModel
    {
        public string QuestionHeader { get; private set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public ObservableCollection<AnswerUIModel> Answers { get; } = new();
        public int SelectedTimerIndex { get; set; }
        public string GroupName { get; }

        public QuestionUIModel(int number)
        {
            GroupName = $"grp{number}";
            for (int i = 0; i < 4; i++)
                Answers.Add(new AnswerUIModel(GroupName));
            UpdateHeader(number);
        }

        public void UpdateHeader(int number)
            => QuestionHeader = $"Вопрос {number}";
    }

    public class AnswerUIModel
    {
        public string GroupName { get; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        public AnswerUIModel(string groupName)
        {
            GroupName = groupName;
        }
    }
}