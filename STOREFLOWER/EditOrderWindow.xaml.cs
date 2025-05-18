using STOREFLOWER.Models;
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
using System.Windows.Shapes;

namespace STOREFLOWER
{
    /// <summary>
    /// Логика взаимодействия для EditOrderWindow.xaml
    /// </summary>
    public partial class EditOrderWindow : Window
    {
        private readonly StoreFlowerContext _context;
        private readonly OrderViewModel _order;
        private readonly List<OrderViewModel> _allOrders;
        private readonly AdminWindow _adminWindow;

        public EditOrderWindow(OrderViewModel order, StoreFlowerContext context, List<OrderViewModel> allOrders, AdminWindow adminWindow)
        {
            InitializeComponent();
            _order = order;
            _context = context;
            _allOrders = allOrders;
            _adminWindow = adminWindow;
            DataContext = _order;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(_order.OrderNumber, out int orderId))
                {
                    var dbOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
                    if (dbOrder != null)
                    {
                        if (dbOrder.StatusID != _order.StatusID)
                        {
                            dbOrder.StatusID = _order.StatusID;
                            _order.Status = _context.OrderStatuses
                                .Where(s => s.StatusID == _order.StatusID)
                                .Select(s => s.StatusName)
                                .FirstOrDefault() ?? "Не указан";
                        }

                        if (_order.FloristId != 0)
                        {
                            if (dbOrder.FloristID != _order.FloristId)
                            {
                                dbOrder.FloristID = _order.FloristId;
                                _order.FloristDisplay = _context.Florists
                                    .Where(f => f.FloristID == _order.FloristId)
                                    .Select(f => $"{f.LastName} {f.FirstName} {f.Patronymic}")
                                    .FirstOrDefault() ?? "Не назначен";
                            }
                        }
                        else
                        {
                            if (dbOrder.FloristID != null)
                            {
                                dbOrder.FloristID = null;
                                _order.FloristDisplay = "Не назначен";
                            }
                        }

                        _context.SaveChanges();

                        var orderInList = _allOrders.FirstOrDefault(o => o.OrderNumber == _order.OrderNumber);
                        if (orderInList != null)
                        {
                            orderInList.StatusID = _order.StatusID;
                            orderInList.Status = _order.Status;
                            orderInList.FloristId = _order.FloristId;
                            orderInList.FloristDisplay = _order.FloristDisplay;
                        }

                        _adminWindow.RefreshOrdersGrid();

                        MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Заказ с номером {_order.OrderNumber} не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Неверный формат номера заказа: {_order.OrderNumber}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}