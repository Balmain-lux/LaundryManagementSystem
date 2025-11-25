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
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private Orders selectedOrder;
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = Connection.entities.Orders
                    .Include("Clients")
                    .Include("Services")
                    .Include("FabricTypes")
                    .Include("OrderStatuses")
                    .OrderByDescending(o => o.CreateDate)
                    .ToList();

                dgOrders.ItemsSource = orders;
            }
            catch (System.Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new NewOrderPage());
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrder == null)
            {
                MainWindow.Instance.ShowError("Выберите заказ для редактирования");
                return;
            }

            MainWindow.Instance.MainFrame.Navigate(new EditOrderPage(selectedOrder));
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrder == null)
            {
                MainWindow.Instance.ShowError("Выберите заказ для удаления");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить заказ №{selectedOrder.OrderID}?\nКлиент: {selectedOrder.Clients.FullName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Удаляем связанные записи (фотографии, уведомления, использование материалов)
                    var photos = Connection.entities.OrderPhotos.Where(p => p.OrderID == selectedOrder.OrderID).ToList();
                    var notifications = Connection.entities.Notifications.Where(n => n.OrderID == selectedOrder.OrderID).ToList();
                    var materialUsage = Connection.entities.MaterialUsage.Where(m => m.OrderID == selectedOrder.OrderID).ToList();

                    Connection.entities.OrderPhotos.RemoveRange(photos);
                    Connection.entities.Notifications.RemoveRange(notifications);
                    Connection.entities.MaterialUsage.RemoveRange(materialUsage);

                    // Удаляем сам заказ
                    Connection.entities.Orders.Remove(selectedOrder);
                    Connection.entities.SaveChanges();

                    MainWindow.Instance.ShowMessage($"Заказ №{selectedOrder.OrderID} успешно удален");
                    LoadOrders();
                    selectedOrder = null;
                }
                catch (System.Exception ex)
                {
                    MainWindow.Instance.ShowError($"Ошибка удаления заказа: {ex.Message}");
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            MainWindow.Instance.ShowMessage("Данные обновлены");
        }

        private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedOrder = dgOrders.SelectedItem as Orders;
        }
    }
}
