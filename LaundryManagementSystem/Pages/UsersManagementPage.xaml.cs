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
    /// Логика взаимодействия для UsersManagementPage.xaml
    /// </summary>
    public partial class UsersManagementPage : Page
    {
        private Users selectedUser;
        public UsersManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = Connection.entities.Users
                    .OrderBy(u => u.Username)
                    .ToList();

                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }


        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new RegisterPage());
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            MainWindow.Instance.ShowMessage("Список пользователей обновлен");
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = dgUsers.SelectedItem as Users;
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int userId)
            {
                var user = Connection.entities.Users.FirstOrDefault(u => u.UserID == userId);
                if (user != null)
                {
                    MainWindow.Instance.MainFrame.Navigate(new EditUserPage(user));
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int userId)
            {
                var user = Connection.entities.Users.FirstOrDefault(u => u.UserID == userId);
                if (user != null)
                {
                    // Нельзя удалить самого себя
                    if (user.UserID == MainWindow.Instance.CurrentUser.UserID)
                    {
                        MainWindow.Instance.ShowError("Нельзя удалить свой собственный аккаунт");
                        return;
                    }

                    // Нельзя удалить последнего администратора
                    if (user.Role == "Admin")
                    {
                        var adminCount = Connection.entities.Users.Count(u => u.Role == "Admin");
                        if (adminCount <= 1)
                        {
                            MainWindow.Instance.ShowError("Нельзя удалить последнего администратора");
                            return;
                        }
                    }

                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить пользователя?\nЛогин: {user.Username}\nФИО: {user.FullName}",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Connection.entities.Users.Remove(user);
                            Connection.entities.SaveChanges();

                            MainWindow.Instance.ShowMessage($"Пользователь {user.Username} успешно удален");
                            LoadUsers();
                        }
                        catch (Exception ex)
                        {
                            MainWindow.Instance.ShowError($"Ошибка удаления пользователя: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
