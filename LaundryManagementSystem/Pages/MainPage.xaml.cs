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
            UserInfo = $"{user.FullName} ({GetRoleDisplayName(user.Role)})";

            // Скрываем кнопки в зависимости от роли
            if (user.Role == "User")
            {
                // Пользователь может только просматривать свои заказы
                btnMaterials.Visibility = Visibility.Collapsed;
                btnReports.Visibility = Visibility.Collapsed;
                btnUsers.Visibility = Visibility.Collapsed;
                btnServices.Visibility = Visibility.Collapsed;
                btnClients.Visibility = Visibility.Collapsed;
                // Только заказы доступны
            }
            else if (user.Role == "Receptionist")
            {
                // Приемщик имеет доступ к заказам, клиентам и услугам
                btnMaterials.Visibility = Visibility.Collapsed;
                btnReports.Visibility = Visibility.Collapsed;
                btnUsers.Visibility = Visibility.Collapsed;
            }
            // Admin имеет полный доступ
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role)
            {
                case "Admin": return "Администратор";
                case "Receptionist": return "Приемщик";
                case "User": return "Пользователь";
                default: return role;
            }
        }

        private void NavigateToOrdersPage()
        {
            var user = MainWindow.Instance.CurrentUser;
            if (user.Role == "User")
            {
                MainFrame.Navigate(new UserOrdersPage());
            }
            else
            {
                MainFrame.Navigate(new OrdersPage());
            }
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
            if (MainWindow.Instance.CurrentUser.Role == "Admin")
            {
                MainWindow.Instance.MainFrame.Navigate(new UsersManagementPage());
            }
            else
            {
                MainWindow.Instance.ShowError("Недостаточно прав для управления пользователями");
            }
        }
    }
}
