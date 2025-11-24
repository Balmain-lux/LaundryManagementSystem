using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LaundryManagementSystem
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                // Создаем экземпляр Application
                Application app = new Application();

                // Создаем главное окно
                MainWindow mainWindow = new MainWindow();

                // Запускаем приложение
                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при запуске: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
