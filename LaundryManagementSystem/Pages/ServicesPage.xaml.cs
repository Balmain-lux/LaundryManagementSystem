using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaundryManagementSystem.Data;

namespace LaundryManagementSystem.Pages
{
    /// <summary>
    /// Логика взаимодействия для ServicesPage.xaml
    /// </summary>
    public partial class ServicesPage : Page
    {
        private Services selectedService;
        public ServicesPage()
        {
            InitializeComponent();
            LoadServices();
        }
        private void LoadServices()
        {
            try
            {
                var services = Connection.entities.Services
                    .OrderBy(s => s.ServiceName)
                    .Select(s => new
                    {
                        s.ServiceID,
                        s.ServiceName,
                        s.BasePrice,
                        s.StandardTerm,
                        s.Description,
                        Popularity = s.Orders.Count
                    })
                    .ToList();

                dgServices.ItemsSource = services;
            }
            catch (System.Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new EditServicePage());
        }

        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedService == null)
            {
                MainWindow.Instance.ShowError("Выберите услугу для редактирования");
                return;
            }

            MainWindow.Instance.MainFrame.Navigate(new EditServicePage(selectedService));
        }

        private void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedService == null)
            {
                MainWindow.Instance.ShowError("Выберите услугу для удаления");
                return;
            }

            // Проверяем, используется ли услуга в заказах
            if (selectedService.Orders.Any())
            {
                MainWindow.Instance.ShowError("Нельзя удалить услугу, которая используется в заказах");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить услугу?\nНазвание: {selectedService.ServiceName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Connection.entities.Services.Remove(selectedService);
                    Connection.entities.SaveChanges();

                    MainWindow.Instance.ShowMessage($"Услуга {selectedService.ServiceName} успешно удалена");
                    LoadServices();
                    selectedService = null;
                }
                catch (System.Exception ex)
                {
                    MainWindow.Instance.ShowError($"Ошибка удаления услуги: {ex.Message}");
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadServices();
            MainWindow.Instance.ShowMessage("Данные обновлены");
        }

        private void GoToMainPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new MainPage());
        }

        private void dgServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgServices.SelectedItem != null)
            {
                var selectedItem = dgServices.SelectedItem;
                var serviceID = (int)selectedItem.GetType().GetProperty("ServiceID").GetValue(selectedItem);
                selectedService = Connection.entities.Services.FirstOrDefault(s => s.ServiceID == serviceID);
            }
        }
    }
}
