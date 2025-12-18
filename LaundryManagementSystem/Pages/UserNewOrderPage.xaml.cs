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
    /// Логика взаимодействия для UserNewOrderPage.xaml
    /// </summary>
    public partial class UserNewOrderPage : Page
    {
        private Clients currentClient;
        private decimal basePrice = 0;
        private decimal coefficient = 1.0m;
        public UserNewOrderPage(Clients client)
        {
            InitializeComponent();
            currentClient = client;
            LoadComboBoxData();
            CalculatePrice();

            // Показываем информацию о клиенте
            txtClientInfo.Text = $"{client.FullName}\nТелефон: {client.Phone}\nБонусы: {client.BonusPoints}";
        }

        private void LoadComboBoxData()
        {
            try
            {
                cmbServices.ItemsSource = Connection.entities.Services
                    .OrderBy(s => s.ServiceName)
                    .ToList();
                cmbFabrics.ItemsSource = Connection.entities.FabricTypes
                    .OrderBy(f => f.FabricName)
                    .ToList();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }



        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cmbServices.SelectedItem == null || cmbFabrics.SelectedItem == null)
            {
                MainWindow.Instance.ShowError("Выберите услугу и тип ткани!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtItemDescription.Text))
            {
                MainWindow.Instance.ShowError("Опишите предмет одежды!");
                return;
            }

            try
            {
                Orders newOrder = new Orders
                {
                    ClientID = currentClient.ClientID,
                    ServiceID = ((Services)cmbServices.SelectedItem).ServiceID,
                    FabricTypeID = ((FabricTypes)cmbFabrics.SelectedItem).FabricTypeID,
                    StatusID = 1, // "принят"
                    Urgent = chkUrgent.IsChecked ?? false,
                    TotalPrice = decimal.Parse(txtPrice.Text.Replace("₽", "").Replace(" ", "").Replace(",", "")),
                    CreateDate = DateTime.Now,
                    Notes = $"Предмет: {txtItemDescription.Text}\nДополнительно: {txtNotes.Text}"
                };

                Connection.entities.Orders.Add(newOrder);
                Connection.entities.SaveChanges();

                // Добавляем бонусные баллы (5 баллов за каждые 100 рублей)
                int bonusPoints = (int)(newOrder.TotalPrice / 100 * 5);
                currentClient.BonusPoints = (currentClient.BonusPoints ?? 0) + bonusPoints;
                Connection.entities.SaveChanges();

                MainWindow.Instance.ShowMessage($"Заказ успешно создан!\n" +
                    $"Номер заказа: {newOrder.OrderID}\n" +
                    $"Стоимость: {txtPrice.Text}\n" +
                    $"Начислено бонусов: {bonusPoints}\n\n" +
                    $"Вы можете отслеживать статус заказа в разделе 'Мои заказы'");

                // Возврат на страницу заказов пользователя
                MainWindow.Instance.MainFrame.Navigate(new UserOrdersPage());
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка создания заказа: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }

        private void cmbServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbServices.SelectedItem is Services selectedService)
            {
                basePrice = selectedService.BasePrice;
                txtServiceDescription.Text = selectedService.Description ?? "Нет описания";
                CalculatePrice();
            }
        }

        private void cmbFabrics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFabrics.SelectedItem is FabricTypes selectedFabric)
            {
                coefficient = selectedFabric.ComplexityCoefficient ?? 1.0m;
                txtFabricDescription.Text = selectedFabric.Description ?? "Нет описания";
                CalculatePrice();
            }
        }

        private void chkUrgent_Checked(object sender, RoutedEventArgs e)
        {
            CalculatePrice();
        }

        private void CalculatePrice()
        {
            decimal price = basePrice * coefficient;

            if (chkUrgent.IsChecked == true)
            {
                price *= 1.3m; // +30% за срочность
            }

            txtPrice.Text = $"{price:C2}";
        }
    }
}
