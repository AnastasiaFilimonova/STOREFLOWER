using Microsoft.EntityFrameworkCore;
using STOREFLOWER.Models;
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
                .Where(s => s.StatusID == 1 || s.StatusID == 2)
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
                OrdersGrid.ItemsSource = _allOrders.Where(o => o.StatusID == 1).ToList();
            }
            else if (selectedFilter == "Оплачены")
            {
                OrdersGrid.ItemsSource = _allOrders.Where(o => o.StatusID == 2).ToList();
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

        private void StatusHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            if (hyperlink == null) return;

            var order = hyperlink.DataContext as OrderViewModel;
            if (order == null) return;

            var editWindow = new EditOrderWindow(order, _context, _allOrders, this);
            editWindow.Owner = this;
            editWindow.ShowDialog();
        }

        private void FloristHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            if (hyperlink == null) return;

            var order = hyperlink.DataContext as OrderViewModel;
            if (order == null) return;

            var editWindow = new EditOrderWindow(order, _context, _allOrders, this);
            editWindow.Owner = this;
            editWindow.ShowDialog();
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOrderWindow createOrderWindow = new CreateOrderWindow(_currentAdmin);
            createOrderWindow.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        public void RefreshOrdersGrid()
        {
            var currentItemsSource = OrdersGrid.ItemsSource;
            OrdersGrid.ItemsSource = null;
            OrdersGrid.ItemsSource = currentItemsSource;
        }
    }
}