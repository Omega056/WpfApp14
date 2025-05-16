using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class StorePage : Page
    {
        public StorePage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}