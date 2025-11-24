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
    /// Логика взаимодействия для NewOrderPage.xaml
    /// </summary>
    public partial class NewOrderPage : Page
    {
        private decimal basePrice = 0;
        private decimal coefficient = 1.0m;

        public NewOrderPage()
        {
            InitializeComponent();
            LoadComboBoxData();
            CalculatePrice();
        }

        private void LoadComboBoxData()
        {
            try
            {
                cmbClients.ItemsSource = Connection.entities.Clients.OrderBy(c => c.FullName).ToList();
                cmbServices.ItemsSource = Connection.entities.Services.OrderBy(s => s.ServiceName).ToList();
                cmbFabrics.ItemsSource = Connection.entities.FabricTypes.OrderBy(f => f.FabricName).ToList();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void cmbServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbServices.SelectedItem is Services selectedService)
            {
                basePrice = selectedService.BasePrice;
                CalculatePrice();
            }
        }

        private void cmbFabrics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFabrics.SelectedItem is FabricTypes selectedFabric)
            {
                coefficient = selectedFabric.ComplexityCoefficient ?? 1.0m;
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

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClients.SelectedItem == null || cmbServices.SelectedItem == null || cmbFabrics.SelectedItem == null)
            {
                MainWindow.Instance.ShowError("Заполните все обязательные поля!");
                return;
            }

            try
            {
                Orders newOrder = new Orders
                {
                    ClientID = ((Clients)cmbClients.SelectedItem).ClientID,
                    ServiceID = ((Services)cmbServices.SelectedItem).ServiceID,
                    FabricTypeID = ((FabricTypes)cmbFabrics.SelectedItem).FabricTypeID,
                    StatusID = 1, // "принят"
                    Urgent = chkUrgent.IsChecked ?? false,
                    TotalPrice = decimal.Parse(txtPrice.Text.Replace("₽", "").Replace(" ", "")),
                    CreateDate = DateTime.Now,
                    Notes = txtNotes.Text
                };

                Connection.entities.Orders.Add(newOrder);
                Connection.entities.SaveChanges();

                MainWindow.Instance.ShowMessage($"Заказ успешно создан!\nНомер заказа: {newOrder.OrderID}\nСтоимость: {txtPrice.Text}");

                // Возврат на страницу заказов
                MainWindow.Instance.MainFrame.GoBack();
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
    }
}
