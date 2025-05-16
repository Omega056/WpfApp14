using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class StorePage : Page
    {
        public StorePage()
        {
            InitializeComponent();

            // Пример инициализации списка товаров
            ItemsControl.ItemsSource = new[]
            {
                new { ImagePath = "lamp.png", Title = "Лампочка (Подсказка)", Price = "$1.99" },
                new { ImagePath = "fifty.png", Title = "50 на 50", Price = "$2.49" },
                new { ImagePath = "secondchance.png", Title = "Второй шанс", Price = "$3.99" },
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика возврата назад
            NavigationService?.GoBack();
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно получить выбранный товар через DataContext кнопки:
            if (sender is Button btn && btn.DataContext != null)
            {
                dynamic item = btn.DataContext;
                MessageBox.Show($"Вы купили «{item.Title}» за {item.Price}!");
            }
        }
    }
}
