using Microsoft.EntityFrameworkCore;
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
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private readonly StoreFlowerContext _context;
        private readonly Admin _currentAdmin;
        private List<OrderViewModel> _allOrders;

        public AdminWindow(Admin admin)
        {
            InitializeComponent();
            _context = new StoreFlowerContext();
            _currentAdmin = admin;
            LoadAdminData();
            LoadFlorists();
            LoadOrders();
        }

        private void LoadAdminData()
        {
            AdminNameTextBlock.Text = $"{_currentAdmin.LastName} {_currentAdmin.FirstName} {_currentAdmin.Patronymic}";
            var store = _context.Stores.FirstOrDefault(s => s.StoreID == _currentAdmin.StoreID);
            StoreAddressTextBlock.Text = store?.Address ?? "Магазин не найден";
        }

        private void LoadFlorists()
        {
            var florists = _context.Florists
                .Where(f => f.StoreID == _currentAdmin.StoreID)
                .ToList()
                .Select(f => new FloristViewModel
                {
                    FullName = $"{f.LastName} {f.FirstName} {f.Patronymic}",
                    Phone = f.PhoneNumber,
                    Address = GetStoreAddress(f.StoreID),
                    Id = f.FloristID
                })
                .ToList();

            FloristListBox.ItemsSource = florists;
        }

        private string GetStoreAddress(int storeId)
        {
            return _context.Stores.FirstOrDefault(s => s.StoreID == storeId)?.Address ?? "Не указан";
        }

        private void LoadOrders()
        {
            var orders = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Florist)
                .Include(o => o.Deliverer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.StoreID == _currentAdmin.StoreID)
                .ToList();

            var availableStatuses = _context.OrderStatuses
                .Select(s => new OrderStatusViewModel
                {
                    Id = s.StatusID,
                    Name = s.StatusName
                })
                .ToList();

            _allOrders = orders
                .Where(o => o.StatusID == 1 || o.StatusID == 2)
                .Select(o => new OrderViewModel
                {
                    OrderNumber = o.OrderID.ToString(),
                    CreationDate = o.OrderDate.ToString("dd.MM.yyyy"),
                    DeliveryAddress = o.DeliveryAddress,
                    DeliveryDateTime = o.DeliveryDateTime,
                    Status = o.Status?.StatusName ?? "Не указан",
                    StatusID = o.StatusID,
                    Customer = $"{o.ClientLastName} {o.ClientFirstName} {o.ClientPatronymic}",
                    ClientPhone = o.ClientPhoneNumber,
                    FloristDisplay = o.Florist != null ? $"{o.Florist.LastName} {o.Florist.FirstName} {o.Florist.Patronymic}" : "Не назначен",
                    DelivererDisplay = o.Deliverer != null ? $"{o.Deliverer.LastName} {o.Deliverer.FirstName} {o.Deliverer.Patronymic}" : "Не назначен",
                    FloristId = o.Florist?.FloristID ?? 0,
                    AvailableFlorists = new List<FloristViewModel>
                    {
                        new FloristViewModel { FullName = "Не назначен", Id = 0 }
                    }.Concat(_context.Florists
                        .Where(f => f.StoreID == _currentAdmin.StoreID)
                        .Select(f => new FloristViewModel
                        {
                            FullName = $"{f.LastName} {f.FirstName} {f.Patronymic}",
                            Id = f.FloristID
                        }))
                        .ToList(),
                    AvailableStatuses = availableStatuses,
                    OrderItems = o.OrderItems?.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product?.ProductName ?? "Не указан",
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList() ?? new List<OrderItemViewModel>()
                })
                .ToList();

            OrdersGrid.ItemsSource = _allOrders;
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allOrders == null) return;

            var selectedFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (selectedFilter == "Все")
            {
                OrdersGrid.ItemsSource = _allOrders;
            }
            else if (selectedFilter == "Ожидают оплаты")
            {
                var filteredOrders = _allOrders
                    .Where(o => o.StatusID == 1)
                    .ToList();
                OrdersGrid.ItemsSource = filteredOrders;
            }
            else if (selectedFilter == "Оплачены")
            {
                var filteredOrders = _allOrders
                    .Where(o => o.StatusID == 2)
                    .ToList();
                OrdersGrid.ItemsSource = filteredOrders;
            }
        }

        private void OrderNumberHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            if (hyperlink == null) return;

            var order = hyperlink.DataContext as OrderViewModel;
            if (order == null) return;

            var detailsWindow = new OrderDetailsWindow(order);
            detailsWindow.ShowDialog();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var order in _allOrders)
                {
                    var dbOrder = _context.Orders.FirstOrDefault(o => o.OrderID == int.Parse(order.OrderNumber));
                    if (dbOrder != null)
                    {
                        if (dbOrder.StatusID != order.StatusID)
                        {
                            dbOrder.StatusID = order.StatusID;
                            order.Status = _context.OrderStatuses
                                .Where(s => s.StatusID == order.StatusID)
                                .Select(s => s.StatusName)
                                .FirstOrDefault() ?? "Не указан";
                        }

                        if (order.FloristId != 0)
                        {
                            if (dbOrder.FloristID != order.FloristId)
                            {
                                dbOrder.FloristID = order.FloristId;
                                order.FloristDisplay = _context.Florists
                                    .Where(f => f.FloristID == order.FloristId)
                                    .Select(f => $"{f.LastName} {f.FirstName} {f.Patronymic}")
                                    .FirstOrDefault() ?? "Не назначен";
                            }
                        }
                        else
                        {
                            if (dbOrder.FloristID != null)
                            {
                                dbOrder.FloristID = null;
                                order.FloristDisplay = "Не назначен";
                            }
                        }
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOrderWindow createOrderWindow = new CreateOrderWindow(_currentAdmin);
            createOrderWindow.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is OrderViewModel order)
            {
                var selectedStatus = comboBox.SelectedValue as OrderStatusViewModel;
                if (selectedStatus != null && order.StatusID != selectedStatus.Id)
                {
                    order.StatusID = selectedStatus.Id;
                    order.Status = selectedStatus.Name;

                    // Обновляем объект в базе данных с отладкой
                    if (int.TryParse(order.OrderNumber, out int orderId))
                    {
                        var dbOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
                        if (dbOrder != null)
                        {
                            dbOrder.StatusID = order.StatusID;
                            try
                            {
                                _context.SaveChanges();
                                MessageBox.Show($"Статус заказа {order.OrderNumber} успешно обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при сохранении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Заказ с номером {order.OrderNumber} не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Неверный формат номера заказа: {order.OrderNumber}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void FloristComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is OrderViewModel order)
            {
                var selectedFlorist = comboBox.SelectedValue as FloristViewModel;
                if (selectedFlorist != null && order.FloristId != selectedFlorist.Id)
                {
                    order.FloristId = selectedFlorist.Id;
                    order.FloristDisplay = selectedFlorist.Id == 0 ? "Не назначен" : selectedFlorist.FullName;

                    // Обновляем объект в базе данных с отладкой
                    if (int.TryParse(order.OrderNumber, out int orderId))
                    {
                        var dbOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
                        if (dbOrder != null)
                        {
                            dbOrder.FloristID = selectedFlorist.Id == 0 ? (int?)null : selectedFlorist.Id;
                            try
                            {
                                _context.SaveChanges();
                                MessageBox.Show($"Флорист для заказа {order.OrderNumber} успешно обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при сохранении флориста: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Заказ с номером {order.OrderNumber} не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Неверный формат номера заказа: {order.OrderNumber}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class OrderViewModel
    {
        public string OrderNumber { get; set; }
        public string CreationDate { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public string Customer { get; set; }
        public string ClientPhone { get; set; }
        public string FloristDisplay { get; set; }
        public string DelivererDisplay { get; set; }
        public int FloristId { get; set; }
        public List<FloristViewModel> AvailableFlorists { get; set; }
        public List<OrderStatusViewModel> AvailableStatuses { get; set; }
        public FloristViewModel SelectedFlorist { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public decimal TotalOrderPrice => OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;
    }

    public class FloristViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Id { get; set; }
    }

    public class OrderStatusViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}