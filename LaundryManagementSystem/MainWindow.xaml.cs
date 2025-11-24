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
using LaundryManagementSystem.Pages;

namespace LaundryManagementSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public Users CurrentUser { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            NavigateToLoginPage();
        }

        public void NavigateToLoginPage()
        {
            MainFrame.Navigate(new LoginPage());
        }

        public void NavigateToMainPage(Users user)
        {
            CurrentUser = user;
            MainFrame.Navigate(new MainPage());
        }

        public void ShowMessage(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
