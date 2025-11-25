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
    /// Логика взаимодействия для EditOrderPage.xaml
    /// </summary>
    public partial class EditOrderPage : Page
    {
        private Orders currentOrder;
        private decimal basePrice = 0;
        private decimal coefficient = 1.0m;

        public EditOrderPage(Orders order)
        {
            InitializeComponent();
            currentOrder = order;
            LoadComboBoxData();
            LoadOrderData();
            CalculatePrice();
        }

        private void LoadComboBoxData()
        {
            try
            {
                cmbClients.ItemsSource = Connection.entities.Clients.OrderBy(c => c.FullName).ToList();
                cmbServices.ItemsSource = Connection.entities.Services.OrderBy(s => s.ServiceName).ToList();
                cmbFabrics.ItemsSource = Connection.entities.FabricTypes.OrderBy(f => f.FabricName).ToList();
                cmbStatuses.ItemsSource = Connection.entities.OrderStatuses.OrderBy(s => s.StatusID).ToList();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
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

        private void LoadOrderData()
        {
            try
            {
                txtOrderNumber.Text = $"№{currentOrder.OrderID}";

                // Загружаем текущие значения заказа
                cmbClients.SelectedValue = currentOrder.ClientID;
                cmbServices.SelectedValue = currentOrder.ServiceID;
                cmbFabrics.SelectedValue = currentOrder.FabricTypeID;
                cmbStatuses.SelectedValue = currentOrder.StatusID;
                chkUrgent.IsChecked = currentOrder.Urgent;
                txtNotes.Text = currentOrder.Notes;

                // Устанавливаем базовые значения для расчета цены
                if (currentOrder.Services != null)
                    basePrice = currentOrder.Services.BasePrice;
                if (currentOrder.FabricTypes != null)
                    coefficient = currentOrder.FabricTypes.ComplexityCoefficient ?? 1.0m;
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки данных заказа: {ex.Message}");
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

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClients.SelectedItem == null || cmbServices.SelectedItem == null ||
                cmbFabrics.SelectedItem == null || cmbStatuses.SelectedItem == null)
            {
                MainWindow.Instance.ShowError("Заполните все обязательные поля!");
                return;
            }

            try
            {
                // Обновляем данные заказа
                currentOrder.ClientID = ((Clients)cmbClients.SelectedItem).ClientID;
                currentOrder.ServiceID = ((Services)cmbServices.SelectedItem).ServiceID;
                currentOrder.FabricTypeID = ((FabricTypes)cmbFabrics.SelectedItem).FabricTypeID;
                currentOrder.StatusID = ((OrderStatuses)cmbStatuses.SelectedItem).StatusID;
                currentOrder.Urgent = chkUrgent.IsChecked ?? false;
                currentOrder.TotalPrice = decimal.Parse(txtPrice.Text.Replace("₽", "").Replace(" ", ""));
                currentOrder.Notes = txtNotes.Text;

                // Если статус "готов" или "выдан", обновляем даты
                var status = ((OrderStatuses)cmbStatuses.SelectedItem).StatusName;
                if (status == "готов" && currentOrder.CompleteDate == null)
                {
                    currentOrder.CompleteDate = DateTime.Now;
                }
                else if (status == "выдан" && currentOrder.IssueDate == null)
                {
                    currentOrder.IssueDate = DateTime.Now;
                }

                Connection.entities.SaveChanges();

                MainWindow.Instance.ShowMessage($"Заказ №{currentOrder.OrderID} успешно обновлен");

                // Возврат на страницу заказов
                MainWindow.Instance.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка обновления заказа: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }
    }
}
