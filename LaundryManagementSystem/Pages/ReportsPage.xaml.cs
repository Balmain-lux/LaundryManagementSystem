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
    /// Логика взаимодействия для ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            ShowIncomeReportl(); // Показываем отчет по доходам по умолчанию
        }

        private void ShowIncomeReport_Click(object sender, RoutedEventArgs e)
        {
            ShowIncomeReportl();
        }

        private void ShowPopularServices_Click(object sender, RoutedEventArgs e)
        {
            ShowPopularServicesl();
        }

        private void ShowPerformanceReport_Click(object sender, RoutedEventArgs e)
        {
            ShowPerformanceReportl();
        }

        private void ShowActiveClients_Click(object sender, RoutedEventArgs e)
        {
            ShowActiveClientsl();
        }

        private void ShowIncomeReportl()
        {
            try
            {
                var incomeReport = Connection.entities.Orders
                    .Where(o => o.CreateDate != null)
                    .GroupBy(o => new { Year = o.CreateDate.Value.Year, Month = o.CreateDate.Value.Month })
                    .Select(g => new
                    {
                        Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalIncome = g.Sum(o => o.TotalPrice),
                        OrderCount = g.Count(),
                        AvgOrderValue = g.Average(o => o.TotalPrice)
                    })
                    .OrderByDescending(x => x.Period)
                    .Take(12) // Последние 12 месяцев
                    .ToList();

                string report = "📊 ОТЧЕТ ПО ДОХОДАМ ПО МЕСЯЦАМ\n";
                report += "==========================================\n\n";

                foreach (var item in incomeReport)
                {
                    report += $"{item.Period:MMMM yyyy}:\n";
                    report += $"  Доход: {item.TotalIncome:C2}\n";
                    report += $"  Заказов: {item.OrderCount}\n";
                    report += $"  Средний чек: {item.AvgOrderValue:C2}\n";
                    report += "------------------------------------------\n";
                }

                var totalIncome = incomeReport.Sum(x => x.TotalIncome);
                var totalOrders = incomeReport.Sum(x => x.OrderCount);
                report += $"\nИТОГО: {totalIncome:C2} ({totalOrders} заказов)";

                txtReportResults.Text = report;
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        private void ShowPopularServicesl()
        {
            try
            {
                var popularServices = Connection.entities.Services
                    .Select(s => new
                    {
                        Service = s.ServiceName,
                        OrderCount = s.Orders.Count,
                        TotalRevenue = s.Orders.Sum(o => o.TotalPrice),
                        AvgPrice = s.Orders.Any() ? s.Orders.Average(o => o.TotalPrice) : 0
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .ToList();

                string report = "📈 ПОПУЛЯРНЫЕ УСЛУГИ\n";
                report += "==========================================\n\n";

                int position = 1;
                foreach (var service in popularServices)
                {
                    report += $"{position}. {service.Service}:\n";
                    report += $"   Заказов: {service.OrderCount}\n";
                    report += $"   Общий доход: {service.TotalRevenue:C2}\n";
                    report += $"   Средняя стоимость: {service.AvgPrice:C2}\n";
                    report += "------------------------------------------\n";
                    position++;
                }

                txtReportResults.Text = report;
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        private void ShowPerformanceReportl()
        {
            try
            {
                var performanceReport = Connection.entities.Orders
                    .Where(o => o.CompleteDate != null && o.CreateDate != null)
                    .ToList() // Сначала получаем данные, потом работаем с ними
                    .Select(o => new
                    {
                        o.OrderID,
                        Service = o.Services?.ServiceName ?? "Неизвестно",
                        DaysToComplete = (o.CompleteDate.Value - o.CreateDate.Value).TotalDays,
                        IsUrgent = o.Urgent ?? false // Преобразуем nullable в обычный bool
                    })
                    .ToList();

                string report = "⏱️ ОТЧЕТ ПО ВРЕМЕНИ ВЫПОЛНЕНИЯ\n";
                report += "==========================================\n\n";

                if (performanceReport.Any())
                {
                    var avgTime = performanceReport.Average(x => x.DaysToComplete);
                    var maxTime = performanceReport.Max(x => x.DaysToComplete);
                    var minTime = performanceReport.Min(x => x.DaysToComplete);

                    // Теперь IsUrgent имеет тип bool, а не bool?
                    var urgentOrders = performanceReport.Where(x => x.IsUrgent).ToList();
                    var regularOrders = performanceReport.Where(x => !x.IsUrgent).ToList();

                    report += $"Общая статистика:\n";
                    report += $"  Среднее время: {avgTime:F1} дней\n";
                    report += $"  Минимальное время: {minTime:F1} дней\n";
                    report += $"  Максимальное время: {maxTime:F1} дней\n";
                    report += $"  Всего заказов: {performanceReport.Count}\n\n";

                    if (urgentOrders.Any())
                    {
                        report += $"Срочные заказы ({urgentOrders.Count}):\n";
                        report += $"  Среднее время: {urgentOrders.Average(x => x.DaysToComplete):F1} дней\n\n";
                    }

                    report += $"Обычные заказы ({regularOrders.Count}):\n";
                    report += $"  Среднее время: {regularOrders.Average(x => x.DaysToComplete):F1} дней\n";

                    txtReportResults.Text = report;
                }
                else
                {
                    txtReportResults.Text = "Нет данных для формирования отчета о времени выполнения";
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        private void ShowActiveClientsl()
        {
            try
            {
                var activeClients = Connection.entities.Clients
                    .Select(c => new
                    {
                        Client = c.FullName,
                        Phone = c.Phone,
                        OrderCount = c.Orders.Count,
                        TotalSpent = c.Orders.Sum(o => o.TotalPrice),
                        LastOrder = c.Orders.OrderByDescending(o => o.CreateDate).FirstOrDefault().CreateDate,
                        BonusPoints = c.BonusPoints
                    })
                    .Where(x => x.OrderCount > 0)
                    .OrderByDescending(x => x.TotalSpent)
                    .Take(20) // Топ-20 клиентов
                    .ToList();

                string report = "👥 АКТИВНЫЕ КЛИЕНТЫ\n";
                report += "==========================================\n\n";

                int position = 1;
                foreach (var client in activeClients)
                {
                    report += $"{position}. {client.Client}\n";
                    report += $"   Телефон: {client.Phone}\n";
                    report += $"   Заказов: {client.OrderCount}\n";
                    report += $"   Потрачено: {client.TotalSpent:C2}\n";
                    report += $"   Бонусы: {client.BonusPoints}\n";
                    if (client.LastOrder.HasValue)
                    {
                        report += $"   Последний заказ: {client.LastOrder.Value:dd.MM.yyyy}\n";
                    }
                    report += "------------------------------------------\n";
                    position++;
                }

                txtReportResults.Text = report;
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowError($"Ошибка формирования отчета: {ex.Message}");
            }
        }
    }
}
