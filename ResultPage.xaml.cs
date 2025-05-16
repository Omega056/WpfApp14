using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class ResultPage : Page
    {
        public ResultPage(int score, int total)
        {
            InitializeComponent();
            ResultText.Text = $"Ваш результат: {score} из {total}";
        }

        private void Home_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new HomePage());
    }
}
