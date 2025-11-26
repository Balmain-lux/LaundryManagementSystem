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
    /// Логика взаимодействия для EditMaterialPage.xaml
    /// </summary>
    public partial class EditMaterialPage : Page
    {
        private Materials currentMaterial;
        private bool isEditMode = false;

        public string PageTitle => isEditMode ? "Редактирование материала" : "Добавление материала";
        public EditMaterialPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EditMaterialPage(Materials material) : this()
        {
            currentMaterial = material;
            isEditMode = true;
            LoadMaterialData();
        }

        private void LoadMaterialData()
        {
            if (currentMaterial != null)
            {
                txtMaterialName.Text = currentMaterial.MaterialName;
                txtCurrentStock.Text = currentMaterial.CurrentStock.ToString();
                txtMinStock.Text = currentMaterial.MinStock.ToString();
                txtUnitPrice.Text = currentMaterial.UnitPrice?.ToString() ?? "";

                // Устанавливаем единицу измерения
                foreach (ComboBoxItem item in cmbUnit.Items)
                {
                    if (item.Content.ToString() == currentMaterial.Unit)
                    {
                        cmbUnit.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtMessage.Text = "";
            txtError.Visibility = Visibility.Visible;
            txtMessage.Visibility = Visibility.Collapsed;
        }

        private void SaveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (isEditMode)
                {
                    // Редактирование существующего материала
                    currentMaterial.MaterialName = txtMaterialName.Text.Trim();
                    currentMaterial.Unit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content.ToString();
                    currentMaterial.CurrentStock = decimal.Parse(txtCurrentStock.Text);
                    currentMaterial.MinStock = decimal.Parse(txtMinStock.Text);
                    currentMaterial.UnitPrice = string.IsNullOrEmpty(txtUnitPrice.Text) ?
                        (decimal?)null : decimal.Parse(txtUnitPrice.Text);
                }
                else
                {
                    // Создание нового материала
                    var newMaterial = new Materials
                    {
                        MaterialName = txtMaterialName.Text.Trim(),
                        Unit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        CurrentStock = decimal.Parse(txtCurrentStock.Text),
                        MinStock = decimal.Parse(txtMinStock.Text),
                        UnitPrice = string.IsNullOrEmpty(txtUnitPrice.Text) ?
                            (decimal?)null : decimal.Parse(txtUnitPrice.Text)
                    };

                    Connection.entities.Materials.Add(newMaterial);
                }

                Connection.entities.SaveChanges();

                string message = isEditMode ?
                    $"Материал {txtMaterialName.Text} успешно обновлен" :
                    $"Материал {txtMaterialName.Text} успешно добавлен";

                MainWindow.Instance.ShowMessage(message);
                MainWindow.Instance.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка сохранения материала: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtMaterialName.Text))
            {
                ShowError("Введите название материала");
                return false;
            }

            if (cmbUnit.SelectedItem == null)
            {
                ShowError("Выберите единицу измерения");
                return false;
            }

            if (!decimal.TryParse(txtCurrentStock.Text, out decimal currentStock) || currentStock < 0)
            {
                ShowError("Введите корректное значение текущего остатка");
                return false;
            }

            if (!decimal.TryParse(txtMinStock.Text, out decimal minStock) || minStock < 0)
            {
                ShowError("Введите корректное значение минимального запаса");
                return false;
            }

            if (!string.IsNullOrEmpty(txtUnitPrice.Text) &&
                !decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice))
            {
                ShowError("Введите корректное значение цены");
                return false;
            }

            // Проверка уникальности названия (только при создании)
            if (!isEditMode && Connection.entities.Materials.Any(m => m.MaterialName == txtMaterialName.Text.Trim()))
            {
                ShowError("Материал с таким названием уже существует");
                return false;
            }

            return true;
        }



        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.GoBack();
        }
    }
}
