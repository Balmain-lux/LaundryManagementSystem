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
    /// Логика взаимодействия для MaterialsPage.xaml
    /// </summary>
    public partial class MaterialsPage : Page
    {
        private Materials selectedMaterial;
        public MaterialsPage()
        {
            InitializeComponent();
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            try
            {
                var materials = Connection.entities.Materials
                    .OrderBy(m => m.MaterialName)
                    .Select(m => new
                    {
                        m.MaterialID,
                        m.MaterialName,
                        m.Unit,
                        m.CurrentStock,
                        m.MinStock,
                        m.UnitPrice,
                    })
                    .ToList();

                dgMaterials.ItemsSource = materials;
            }
            catch (System.Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки материалов: {ex.Message}");
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new EditMaterialPage());
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial == null)
            {
                MainWindow.Instance.ShowError("Выберите материал для редактирования");
                return;
            }

            MainWindow.Instance.MainFrame.Navigate(new EditMaterialPage(selectedMaterial));
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial == null)
            {
                MainWindow.Instance.ShowError("Выберите материал для удаления");
                return;
            }

            // Проверяем, используется ли материал в заказах
            var materialUsage = Connection.entities.MaterialUsage
                .Any(mu => mu.MaterialID == selectedMaterial.MaterialID);

            if (materialUsage)
            {
                MainWindow.Instance.ShowError("Нельзя удалить материал, который используется в заказах");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить материал?\nНазвание: {selectedMaterial.MaterialName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Connection.entities.Materials.Remove(selectedMaterial);
                    Connection.entities.SaveChanges();

                    MainWindow.Instance.ShowMessage($"Материал {selectedMaterial.MaterialName} успешно удален");
                    LoadMaterials();
                    selectedMaterial = null;
                }
                catch (System.Exception ex)
                {
                    MainWindow.Instance.ShowError($"Ошибка удаления материала: {ex.Message}");
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMaterials();
            MainWindow.Instance.ShowMessage("Данные обновлены");
        }

        private void dgMaterials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMaterials.SelectedItem != null)
            {
                var selectedItem = dgMaterials.SelectedItem;
                var materialID = (int)selectedItem.GetType().GetProperty("MaterialID").GetValue(selectedItem);
                selectedMaterial = Connection.entities.Materials.FirstOrDefault(m => m.MaterialID == materialID);
            }
        }

        private string GetUnit(string materialName)
        {
            var material = Connection.entities.Materials.FirstOrDefault(m => m.MaterialName == materialName);
            return material?.Unit ?? "ед.";
        }

        private void GoToMainPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(new MainPage());
        }
    }
}
