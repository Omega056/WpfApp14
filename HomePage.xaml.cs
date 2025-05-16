using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quiz.db");
            DatabaseService.Initialize(dbPath);
            MessageBox.Show($"DB path: {dbPath}", "Debug");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new SettingsPage());

        private void StoreButton_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new StorePage());

        private void CreateGameButton_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new CreateGamePage());

        private void FindGameButton_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new FindGamePage());
    }
}
