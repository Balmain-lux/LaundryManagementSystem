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

            // Проверяем телефон (логин должен быть в формате телефона для пользователей)
            if (role == "User")
            {
                // Убираем все нецифровые символы для проверки
                string digitsOnly = new string(username.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length < 10)
                {
                    ShowError("Для роли 'Пользователь' логин должен быть номером телефона (минимум 10 цифр)");
                    return;
                }
                // Нормализуем номер телефона (оставляем только цифры)
                username = digitsOnly;
            }

            try
            {
                // Используем транзакцию для надежности
                using (var transaction = Connection.entities.Database.BeginTransaction())
                {
                    try
                    {
                        // Проверка уникальности логина в таблице Users
                        if (Connection.entities.Users.Any(u => u.Username == username))
                        {
                            ShowError("Пользователь с таким логином уже существует");
                            return;
                        }

                        // Создание нового пользователя
                        Users newUser = new Users
                        {
                            Username = username,
                            Password = password,
                            Role = role,
                            FullName = fullName,
                            CreatedDate = DateTime.Now
                        };

                        // Сохранение пользователя
                        Connection.entities.Users.Add(newUser);
                        Connection.entities.SaveChanges();

                        // Если это пользователь, создаем или обновляем клиентскую запись
                        if (role == "User")
                        {
                            // Проверяем, есть ли уже клиент с таким телефоном
                            var existingClient = Connection.entities.Clients
                                .FirstOrDefault(c => c.Phone == username);

                            if (existingClient == null)
                            {
                                // Создаем новую клиентскую запись
                                var client = new Clients
                                {
                                    FullName = fullName,
                                    Phone = username,
                                    Email = null,
                                    BonusPoints = 0,
                                    RegistrationDate = DateTime.Now
                                };

                                Connection.entities.Clients.Add(client);
                            }
                            else
                            {
                                // Если клиент уже существует, обновляем его данные
                                existingClient.FullName = fullName;
                                // Не меняем дату регистрации, если клиент уже был
                                if (!existingClient.RegistrationDate.HasValue)
                                    existingClient.RegistrationDate = DateTime.Now;
                            }

                            Connection.entities.SaveChanges();
                        }

                        // Фиксируем транзакцию
                        transaction.Commit();

                        ShowSuccess($"Пользователь {username} успешно зарегистрирован!\nРоль: {GetRoleDisplayName(role)}");

                        // Очистка полей после успешной регистрации
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        // Откатываем транзакцию при ошибке
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // Более подробное сообщение об ошибке
                string errorMessage = GetDetailedErrorMessage(ex);
                ShowError(errorMessage);
            }
        }
        private string GetDetailedErrorMessage(Exception ex)
        {
            string errorMessage = $"Ошибка регистрации: {ex.Message}";

            // Рекурсивно получаем все внутренние исключения
            Exception innerEx = ex.InnerException;
            int level = 1;

            while (innerEx != null && level <= 3) // Ограничиваем глубину 3 уровнями
            {
                errorMessage += $"\nВнутренняя ошибка {level}: {innerEx.Message}";
                innerEx = innerEx.InnerException;
                level++;
            }

            return errorMessage;
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
