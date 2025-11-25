using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace LaundryManagementSystem.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        private string _userInfo;

        public string UserInfo
        {
            get { return _userInfo; }
            set
            {
                _userInfo = value;
                OnPropertyChanged(nameof(UserInfo));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            InitializeUserInfo();
            NavigateToOrdersPage();

        }
        private void InitializeUserInfo()
        {
            var user = MainWindow.Instance.CurrentUser;
            UserInfo = $"{user.FullName} ({user.Role})";

            // Скрываем кнопки в зависимости от роли
            if (user.Role != "Admin")
            {
                btnMaterials.Visibility = Visibility.Collapsed;
                btnReports.Visibility = Visibility.Collapsed;
            }
        }
        private void NavigateToOrdersPage()
        {
            MainFrame.Navigate(new OrdersPage());
        }
        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigateToOrdersPage();
        }

        private void btnClients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage());
        }

        private void btnServices_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ServicesPage());
        }

        private void btnMaterials_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.CurrentUser.Role == "Admin")
            {
                MainFrame.Navigate(new MaterialsPage());
            }
            else
            {
                MainWindow.Instance.ShowError("Недостаточно прав для просмотра материалов");
            }
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.CurrentUser.Role == "Admin")
            {
                MainFrame.Navigate(new ReportsPage());
            }
            else
            {
                MainWindow.Instance.ShowError("Недостаточно прав для просмотра отчетов");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToLoginPage();
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new UsersManagementPage());
        }
    }
}
