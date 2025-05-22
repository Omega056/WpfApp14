using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfApp14.UIModels
{
    public class QuestionUIModel : INotifyPropertyChanged
    {
        private string _questionHeader;
        private string _questionText;
        private int _selectedTimerIndex;

        public QuestionUIModel(int index)
        {
            UpdateHeader(index);
            Answers = new ObservableCollection<AnswerUIModel>
            {
                new AnswerUIModel { GroupName = $"Question_{index}" },
                new AnswerUIModel { GroupName = $"Question_{index}" },
                new AnswerUIModel { GroupName = $"Question_{index}" },
                new AnswerUIModel { GroupName = $"Question_{index}" }
            };
        }

        public string QuestionHeader
        {
            get => _questionHeader;
            set
            {
                _questionHeader = value;
                OnPropertyChanged(nameof(QuestionHeader));
            }
        }

        public string QuestionText
        {
            get => _questionText;
            set
            {
                _questionText = value;
                OnPropertyChanged(nameof(QuestionText));
            }
        }

        public ObservableCollection<AnswerUIModel> Answers { get; set; }

        public int SelectedTimerIndex
        {
            get => _selectedTimerIndex;
            set
            {
                _selectedTimerIndex = value;
                OnPropertyChanged(nameof(SelectedTimerIndex));
            }
        }

        public void UpdateHeader(int index)
        {
            QuestionHeader = $"Вопрос {index}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnswerUIModel : INotifyPropertyChanged
    {
        private string _text;
        private bool _isCorrect;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                _isCorrect = value;
                OnPropertyChanged(nameof(IsCorrect));
            }
        }

        public string GroupName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}