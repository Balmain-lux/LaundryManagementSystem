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
                        StockStatus = m.CurrentStock <= m.MinStock ? "НИЗКИЙ" :
                                     m.CurrentStock <= m.MinStock * 2 ? "НОРМА" : "ВНИМАНИЕ"
                    })
                    .ToList();

                dgMaterials.ItemsSource = materials;
            }
            catch (System.Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки материалов: {ex.Message}");
            }
        }

        private void dgMaterials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора материала
        }

        private void ShowUsageStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var usageStats = Connection.entities.MaterialUsage
                    .GroupBy(mu => mu.Materials.MaterialName)
                    .Select(g => new
                    {
                        Material = g.Key,
                        TotalUsed = g.Sum(x => x.Quantity),
                        LastUsed = g.Max(x => x.UsageDate)
                    })
                    .OrderByDescending(x => x.TotalUsed)
                    .ToList();

                string statsMessage = "Статистика использования материалов:\n\n";
                foreach (var stat in usageStats)
                {
                    statsMessage += $"{stat.Material}: {stat.TotalUsed} {GetUnit(stat.Material)}\n";
                }

                MainWindow.Instance.ShowMessage(statsMessage, "Статистика использования");
            }
            catch (System.Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка загрузки статистики: {ex.Message}");
            }
        }
        private string GetUnit(string materialName)
        {
            var material = Connection.entities.Materials.FirstOrDefault(m => m.MaterialName == materialName);
            return material?.Unit ?? "ед.";
        }
    }
}
