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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Устанавливаем роль "User" по умолчанию, если не выбрана
            if (string.IsNullOrEmpty(role))
            {
                role = "User";
            }

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) ||
               string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Заполните все обязательные поля");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Пароль должен содержать минимум 4 символа");
                return;
            }

            if (!IsValidUsername(username))
            {
                ShowError("Логин может содержать только буквы, цифры и символы подчеркивания");
                return;
            }

            // Проверяем телефон (логин должен быть в формате телефона для пользователей)
            if (role == "User" && !IsValidPhone(username))
            {
                ShowError("Для роли 'Пользователь' логин должен быть номером телефона (минимум 10 цифр)");
                return;
            }

            // Проверка уникальности логина
            if (Connection.entities.Users.Any(u => u.Username == username))
            {
                ShowError("Пользователь с таким логином уже существует");
                return;
            }
            try
            {
                // Создание нового пользователя
                Users newUser = new Users
                {
                    Username = username,
                    Password = password,
                    Role = role,
                    FullName = fullName,
                    CreatedDate = DateTime.Now
                };

                // Сохранение в базу данных
                Connection.entities.Users.Add(newUser);
                Connection.entities.SaveChanges();

                // Если это пользователь, создаем клиентскую запись
                if (role == "User")
                {
                    var client = new Clients
                    {
                        FullName = fullName,
                        Phone = username,
                        Email = null,
                        BonusPoints = 0,
                        RegistrationDate = DateTime.Now
                    };

                    Connection.entities.Clients.Add(client);
                    Connection.entities.SaveChanges();
                }

                ShowSuccess($"Пользователь {username} успешно зарегистрирован!\nРоль: {GetRoleDisplayName(role)}");

                // Очистка полей после успешной регистрации
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        private bool IsValidUsername(string username)
        {
            // Логин должен содержать только буквы, цифры и символы подчеркивания
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$");
        }

        private bool IsValidPhone(string phone)
        {
            // Простая валидация телефона - минимум 10 цифр
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.Length >= 10;
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role)
            {
                case "Admin": return "Администратор";
                case "Receptionist": return "Приемщик";
                case "User": return "Пользователь (клиент)";
                default: return role;
            }
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

        private void ClearForm()
        {
            txtFullName.Text = "";
            txtUsername.Text = "";
            txtPassword.Password = "";
            txtConfirmPassword.Password = "";
            cmbRole.SelectedIndex = 0;
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }
    }
}
