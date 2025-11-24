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
    }
}
