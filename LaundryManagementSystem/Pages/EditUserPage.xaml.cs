using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для EditUserPage.xaml
    /// </summary>
    public partial class EditUserPage : Page
    {
        private Users currentUser;
        public EditUserPage(Users user)
        {
            InitializeComponent();
            currentUser = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                txtUserID.Text = $"ID: {currentUser.UserID}";
                txtFullName.Text = currentUser.FullName;
                txtUsername.Text = currentUser.Username;

                // Устанавливаем выбранную роль
                foreach (ComboBoxItem item in cmbRole.Items)
                {
                    if (item.Content.ToString() == currentUser.Role)
                    {
                        cmbRole.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки данных пользователя: {ex.Message}");
            }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Валидация данных
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
            {
                ShowError("Заполните все обязательные поля");
                return;
            }

            if (!IsValidUsername(username))
            {
                ShowError("Логин может содержать только буквы, цифры и символы подчеркивания");
                return;
            }

            // Проверка уникальности логина (исключая текущего пользователя)
            if (Connection.entities.Users.Any(u => u.Username == username && u.UserID != currentUser.UserID))
            {
                ShowError("Пользователь с таким логином уже существует");
                return;
            }

            // Проверка на последнего администратора
            if (currentUser.Role == "Admin" && role != "Admin")
            {
                var adminCount = Connection.entities.Users.Count(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    ShowError("Нельзя изменить роль последнего администратора");
                    return;
                }
            }

            try
            {
                // Обновляем данные пользователя
                currentUser.FullName = fullName;
                currentUser.Username = username;
                currentUser.Role = role;

                // Обновляем пароль только если он указан
                if (!string.IsNullOrEmpty(password))
                {
                    if (password.Length < 4)
                    {
                        ShowError("Пароль должен содержать минимум 4 символа");
                        return;
                    }
                    currentUser.Password = password;
                }

                Connection.entities.SaveChanges();

                ShowSuccess($"Данные пользователя {username} успешно обновлены");

                // Если редактируется текущий пользователь, обновляем информацию в главном окне
                if (currentUser.UserID == MainWindow.Instance.CurrentUser.UserID)
                {
                    MainWindow.Instance.CurrentUser = currentUser;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка обновления пользователя: {ex.Message}");
            }
        }

        private bool IsValidUsername(string username)
        {
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$");
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtMessage.Text = "";
            txtError.Visibility = Visibility.Visible;
            txtMessage.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string message)
        {
            txtMessage.Text = message;
            txtError.Text = "";
            txtMessage.Visibility = Visibility.Visible;
            txtError.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }
    }
}
