using System;
using System.IO;
using System.Windows;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class App : Application
    {
        public DatabaseService.User? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quiz.db");
            try
            {
                DatabaseService.Initialize(dbPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}