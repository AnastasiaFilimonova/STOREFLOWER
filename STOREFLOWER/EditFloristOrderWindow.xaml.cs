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
    /// Логика взаимодействия для EditFloristOrderWindow.xaml
    /// </summary>
    public partial class EditFloristOrderWindow : Window
    {
        private readonly StoreFlowerContext _context;
        private readonly OrderViewModel _order;
        private readonly List<OrderViewModel> _allOrders;
        private readonly FloristWindow _floristWindow;
        public EditFloristOrderWindow(OrderViewModel order, StoreFlowerContext context, List<OrderViewModel> allOrders, FloristWindow floristWindow)
        {
            InitializeComponent();
            _order = order;
            _context = context;
            _allOrders = allOrders;
            _floristWindow = floristWindow;
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

                        _context.SaveChanges();

                        var orderInList = _allOrders.FirstOrDefault(o => o.OrderNumber == _order.OrderNumber);
                        if (orderInList != null)
                        {
                            orderInList.StatusID = _order.StatusID;
                            orderInList.Status = _order.Status;
                        }

                        _floristWindow.RefreshOrdersGrid();

                        MessageBox.Show("Статус заказа успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}