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
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        private Clients selectedClient;
        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
        }
        private void LoadClients()
        {
            try
            {
                var clients = Connection.entities.Clients
                    .OrderBy(c => c.FullName)
                    .Select(c => new
                    {
                        c.ClientID,
                        c.FullName,
                        c.Phone,
                        c.Email,
                        c.BonusPoints,
                        c.RegistrationDate,
                        OrdersCount = c.Orders.Count
                    })
                    .ToList();

                dgClients.ItemsSource = clients;
            }
            catch (System.Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new EditClientPage());
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClient == null)
            {
                MainWindow.Instance.ShowError("Выберите клиента для редактирования");
                return;
            }

            MainWindow.Instance.MainFrame.Navigate(new EditClientPage(selectedClient));
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClient == null)
            {
                MainWindow.Instance.ShowError("Выберите клиента для удаления");
                return;
            }

            // Проверяем, есть ли заказы у клиента
            if (selectedClient.Orders.Any())
            {
                MainWindow.Instance.ShowError("Нельзя удалить клиента, у которого есть заказы");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить клиента?\nФИО: {selectedClient.FullName}\nТелефон: {selectedClient.Phone}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Connection.entities.Clients.Remove(selectedClient);
                    Connection.entities.SaveChanges();

                    MainWindow.Instance.ShowMessage($"Клиент {selectedClient.FullName} успешно удален");
                    LoadClients();
                    selectedClient = null;
                }
                catch (System.Exception ex)
                {
                    MainWindow.Instance.ShowError($"Ошибка удаления клиента: {ex.Message}");
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClients();
            MainWindow.Instance.ShowMessage("Данные обновлены");
        }

        private void GoToMainPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new MainPage());
        }

        private void dgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClients.SelectedItem != null)
            {
                var selectedItem = dgClients.SelectedItem;
                var clientID = (int)selectedItem.GetType().GetProperty("ClientID").GetValue(selectedItem);
                selectedClient = Connection.entities.Clients.FirstOrDefault(c => c.ClientID == clientID);
            }
        }
    }
}
