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
    }
}
