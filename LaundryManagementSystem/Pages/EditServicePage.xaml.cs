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
using LaundryManagementSystem.Data;

namespace LaundryManagementSystem.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditServicePage.xaml
    /// </summary>
    public partial class EditServicePage : Page, INotifyPropertyChanged
    {
        private Services currentService;
        private bool isEditMode = false;

        public string PageTitle => isEditMode ? "Редактирование услуги" : "Добавление услуги";

        public event PropertyChangedEventHandler PropertyChanged;
        public EditServicePage()
        {
            InitializeComponent();
            DataContext = this; 
        }
        public EditServicePage(Services service) : this()
        {
            currentService = service;
            isEditMode = true;
            LoadServiceData();
        }

        private void LoadServiceData()
        {
            if (currentService != null)
            {
                txtServiceName.Text = currentService.ServiceName;
                txtBasePrice.Text = currentService.BasePrice.ToString("F2");
                txtStandardTerm.Text = currentService.StandardTerm.ToString();
                txtDescription.Text = currentService.Description ?? "";

                // Показываем информацию об услуге
                txtServiceInfo.Text = $"Заказов: {currentService.Orders.Count}";
            }
        }

        private void SaveService_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (isEditMode)
                {
                    // Редактирование существующей услуги
                    currentService.ServiceName = txtServiceName.Text.Trim();
                    currentService.BasePrice = decimal.Parse(txtBasePrice.Text);
                    currentService.StandardTerm = int.Parse(txtStandardTerm.Text);
                    currentService.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ?
                        null : txtDescription.Text.Trim();
                }
                else
                {
                    // Создание новой услуги
                    var newService = new Services
                    {
                        ServiceName = txtServiceName.Text.Trim(),
                        BasePrice = decimal.Parse(txtBasePrice.Text),
                        StandardTerm = int.Parse(txtStandardTerm.Text),
                        Description = string.IsNullOrWhiteSpace(txtDescription.Text) ?
                            null : txtDescription.Text.Trim()
                    };

                    Connection.entities.Services.Add(newService);
                }

                Connection.entities.SaveChanges();

                string message = isEditMode ?
                    $"Услуга {txtServiceName.Text} успешно обновлена" :
                    $"Услуга {txtServiceName.Text} успешно добавлена";

                MainWindow.Instance.ShowMessage(message);
                MainWindow.Instance.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка сохранения услуги: {ex.Message}");
            }
        }
        private bool ValidateInput()
        {
            // Проверка названия услуги
            if (string.IsNullOrWhiteSpace(txtServiceName.Text))
            {
                ShowError("Введите название услуги");
                return false;
            }

            if (txtServiceName.Text.Trim().Length < 2)
            {
                ShowError("Название услуги должно содержать минимум 2 символа");
                return false;
            }

            // Проверка цены
            if (!decimal.TryParse(txtBasePrice.Text, out decimal basePrice) || basePrice < 0)
            {
                ShowError("Введите корректную базовую цену");
                return false;
            }

            // Проверка срока выполнения
            if (!int.TryParse(txtStandardTerm.Text, out int standardTerm) || standardTerm <= 0)
            {
                ShowError("Введите корректный срок выполнения (должен быть больше 0)");
                return false;
            }

            // Проверка уникальности названия (только при создании)
            if (!isEditMode && Connection.entities.Services.Any(s => s.ServiceName == txtServiceName.Text.Trim()))
            {
                ShowError("Услуга с таким названием уже существует");
                return false;
            }

            return true;
        }
        private void ShowError(string message)
        {
            txtError.Text = message;
            txtMessage.Text = "";
            txtError.Visibility = Visibility.Visible;
            txtMessage.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
