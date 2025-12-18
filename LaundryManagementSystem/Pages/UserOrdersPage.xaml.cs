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
    /// Логика взаимодействия для UserOrdersPage.xaml
    /// </summary>
    public partial class UserOrdersPage : Page
    {
        private Orders selectedOrder;
        public UserOrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var currentUser = MainWindow.Instance.CurrentUser;

                // Пользователь может видеть только свои заказы
                var orders = Connection.entities.Orders
                    .Include("Clients")
                    .Include("Services")
                    .Include("FabricTypes")
                    .Include("OrderStatuses")
                    .Where(o => o.Clients != null && o.Clients.Phone == currentUser.Username)
                    .OrderByDescending(o => o.CreateDate)
                    .ToList();

                dgOrders.ItemsSource = orders;

                // Показываем статистику
                if (orders.Any())
                {
                    var totalSpent = orders.Sum(o => o.TotalPrice);
                    var activeOrders = orders.Count(o => o.OrderStatuses.StatusName != "выдан" &&
                                                         o.OrderStatuses.StatusName != "отменен");
                    var completedOrders = orders.Count(o => o.OrderStatuses.StatusName == "готов" ||
                                                            o.OrderStatuses.StatusName == "выдан");

                    txtStats.Text = $"Всего заказов: {orders.Count} | Активных: {activeOrders} | Выполнено: {completedOrders} | Потрачено: {totalSpent:C2}";
                }
                else
                {
                    txtStats.Text = "У вас пока нет заказов";
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли у пользователя клиентская запись
            var currentUser = MainWindow.Instance.CurrentUser;
            var client = Connection.entities.Clients.FirstOrDefault(c => c.Phone == currentUser.Username);

            if (client == null)
            {
                // Создаем клиентскую запись для пользователя
                client = new Clients
                {
                    FullName = currentUser.FullName,
                    Phone = currentUser.Username,
                    Email = null,
                    BonusPoints = 0,
                    RegistrationDate = DateTime.Now
                };

                Connection.entities.Clients.Add(client);
                Connection.entities.SaveChanges();

                MainWindow.Instance.ShowMessage($"Создана клиентская запись для {currentUser.FullName}");
            }

            MainWindow.Instance.MainFrame.Navigate(new UserNewOrderPage(client));
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            MainWindow.Instance.ShowMessage("Данные обновлены");
        }

        private void GoToMainPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new MainPage());
        }

        private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedOrder = dgOrders.SelectedItem as Orders;
        }
    }
}
