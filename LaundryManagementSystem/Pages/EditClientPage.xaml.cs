using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для EditClientPage.xaml
    /// </summary>
    public partial class EditClientPage : Page, INotifyPropertyChanged
    {
        private Clients currentClient;
        private bool isEditMode = false;

        public string PageTitle => isEditMode ? "Редактирование клиента" : "Добавление клиента";

        public event PropertyChangedEventHandler PropertyChanged;

        public EditClientPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EditClientPage(Clients client) : this()
        {
            currentClient = client;
            isEditMode = true;
            LoadClientData();
        }

        private void LoadClientData()
        {
            if (currentClient != null)
            {
                txtFullName.Text = currentClient.FullName;
                txtPhone.Text = currentClient.Phone;
                txtEmail.Text = currentClient.Email ?? "";
                txtBonusPoints.Text = currentClient.BonusPoints.ToString();

                // Показываем информацию о клиенте
                txtClientInfo.Text = $"Заказов: {currentClient.Orders.Count}\n" +
                                   $"Дата регистрации: {currentClient.RegistrationDate:dd.MM.yyyy}";
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Простая валидация телефона - минимум 10 цифр
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.Length >= 10;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtMessage.Text = "";
            txtError.Visibility = Visibility.Visible;
            txtMessage.Visibility = Visibility.Collapsed;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool ValidateInput()
        {
            // Проверка ФИО
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Введите ФИО клиента");
                return false;
            }

            if (txtFullName.Text.Trim().Length < 2)
            {
                ShowError("ФИО должно содержать минимум 2 символа");
                return false;
            }

            // Проверка телефона
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                ShowError("Введите номер телефона");
                return false;
            }

            string phone = txtPhone.Text.Trim();
            if (!IsValidPhone(phone))
            {
                ShowError("Введите корректный номер телефона");
                return false;
            }

            // Проверка уникальности телефона (только при создании)
            if (!isEditMode && Connection.entities.Clients.Any(c => c.Phone == phone))
            {
                ShowError("Клиент с таким номером телефона уже существует");
                return false;
            }

            // Проверка email (если указан)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text.Trim()))
            {
                ShowError("Введите корректный email адрес");
                return false;
            }

            // Проверка бонусных баллов
            if (!int.TryParse(txtBonusPoints.Text, out int bonusPoints) || bonusPoints < 0)
            {
                ShowError("Введите корректное количество бонусных баллов");
                return false;
            }

            return true;
        }

        private void SaveClient_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (isEditMode)
                {
                    // Редактирование существующего клиента
                    currentClient.FullName = txtFullName.Text.Trim();
                    currentClient.Phone = txtPhone.Text.Trim();
                    currentClient.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                    currentClient.BonusPoints = int.Parse(txtBonusPoints.Text);
                }
                else
                {
                    // Создание нового клиента
                    var newClient = new Clients
                    {
                        FullName = txtFullName.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                        BonusPoints = int.Parse(txtBonusPoints.Text),
                        RegistrationDate = DateTime.Now
                    };

                    Connection.entities.Clients.Add(newClient);
                }

                Connection.entities.SaveChanges();

                string message = isEditMode ?
                    $"Клиент {txtFullName.Text} успешно обновлен" :
                    $"Клиент {txtFullName.Text} успешно добавлен";

                MainWindow.Instance.ShowMessage(message);
                MainWindow.Instance.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка сохранения клиента: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }
    }
}
