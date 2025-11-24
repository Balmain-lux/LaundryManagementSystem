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
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу создания заказа
            MainWindow.Instance.MainFrame.Navigate(new NewOrderPage());
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            MainWindow.Instance.ShowMessage("Данные обновлены");
        }

        private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора заказа
        }
    }
}
