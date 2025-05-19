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
    /// Логика взаимодействия для DeliverWindow.xaml
    /// </summary>
    public partial class DeliverWindow : Window
    {
        private readonly StoreFlowerContext _context;
        private readonly Deliverer _currentDeliverer;
        private List<OrderViewModel> _allOrders;
        private ListCollectionView _ordersView;
        public DeliverWindow(Deliverer deliverer)
        {
            InitializeComponent();
            _context = new StoreFlowerContext();
            _currentDeliverer = deliverer;
            LoadDelivererData();
            LoadShops();
            LoadOrders();
        }

        private void LoadDelivererData()
        {
            var delivererNameTextBlock = (TextBlock)FindName("FloristNameTextBlock");
            if (delivererNameTextBlock == null)
            {
                delivererNameTextBlock = new TextBlock();
                delivererNameTextBlock.Name = "FloristNameTextBlock";
                var stackPanel = (StackPanel)FindName("DelivererInfoStackPanel");
                if (stackPanel != null)
                    stackPanel.Children.Add(delivererNameTextBlock);
            }
            delivererNameTextBlock.Text = $"{_currentDeliverer.LastName} {_currentDeliverer.FirstName} {_currentDeliverer.Patronymic}";

            var storeAddressTextBlock = (TextBlock)FindName("StoreAddressTextBlock");
            if (storeAddressTextBlock != null)
                storeAddressTextBlock.Text = "Доставщик не привязан к магазину";
        }

        private void LoadShops()
        {
            var shops = _context.Stores
                .ToList()
                .Select(s => new ShopViewModel
                {
                    Name = $"Магазин {s.StoreID}",
                    Address = s.Address ?? "Не указан"
                })
                .ToList();

            ShopsListBox.ItemsSource = shops;
        }

        private void LoadOrders()
        {
            var orders = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Florist)
                .Include(o => o.Deliverer)
                .Where(o => o.StatusID == 3 || o.StatusID == 4 || o.StatusID == 5)
                .ToList();

            var availableStatuses = _context.OrderStatuses
                .Where(s => s.StatusID == 3 || s.StatusID == 4 || s.StatusID == 5)
                .Select(s => new OrderStatusViewModel
                {
                    Id = s.StatusID,
                    Name = s.StatusName
                })
                .ToList();

            _allOrders = orders
                .Select(o => new OrderViewModel
                {
                    OrderNumber = o.OrderID.ToString(),
                    CreationDateTime = o.OrderDate,
                    CreationDate = o.OrderDate.ToString("dd.MM.yyyy"),
                    DeliveryAddress = o.DeliveryAddress,
                    DeliveryDateTime = o.DeliveryDateTime,
                    Status = o.Status?.StatusName ?? "Не указан",
                    StatusID = o.StatusID,
                    Customer = $"{o.ClientLastName} {o.ClientFirstName} {o.ClientPatronymic}",
                    ClientPhone = o.ClientPhoneNumber,
                    FloristDisplay = o.Florist != null ? $"{o.Florist.LastName} {o.Florist.FirstName} {o.Florist.Patronymic}" : "Не назначен",
                    DelivererDisplay = o.Deliverer != null ? $"{o.Deliverer.LastName} {o.Deliverer.FirstName} {o.Deliverer.Patronymic}" : "Не назначен",
                    FloristId = o.FloristID ?? 0,
                    AvailableFlorists = null,
                    AvailableStatuses = availableStatuses,
                    OrderItems = o.OrderItems?.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product?.ProductName ?? "Не указан",
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList() ?? new List<OrderItemViewModel>(),
                    StoreID = o.StoreID
                })
                .ToList();

            _ordersView = new ListCollectionView(_allOrders);
            OrdersGrid.ItemsSource = _ordersView;
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allOrders == null || _ordersView == null) return;

            var selectedFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            _ordersView.Filter = null;

            if (selectedFilter == "Все")
            {
                _ordersView.Filter = null;
            }
            else if (selectedFilter == "Готовы к доставке")
            {
                _ordersView.Filter = (obj) => (obj as OrderViewModel)?.StatusID == 3;
            }
            else if (selectedFilter == "Доставлены")
            {
                _ordersView.Filter = (obj) => (obj as OrderViewModel)?.StatusID == 4;
            }
            else if (selectedFilter == "Возврат в магазин")
            {
                _ordersView.Filter = (obj) => (obj as OrderViewModel)?.StatusID == 5;
            }

            _ordersView.Refresh();
        }

        private void OrdersGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var currentFilter = _ordersView.Filter;
            _ordersView.Filter = currentFilter;
        }

        private void OrderNumber_Click(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock == null) return;

            var order = textBlock.DataContext as OrderViewModel;
            if (order == null) return;

            var detailsWindow = new DeliveryDetailsWindow(order, _context);
            detailsWindow.ShowDialog();
        }

        private void Status_Click(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock == null) return;

            var orderViewModel = textBlock.DataContext as OrderViewModel;
            if (orderViewModel == null) return;

            var editWindow = new DeliveryDetailsWindow(orderViewModel, _context);
            editWindow.ShowDialog();

            if (editWindow.IsStatusChanged)
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderID.ToString() == orderViewModel.OrderNumber);
                if (order != null)
                {
                    order.StatusID = orderViewModel.StatusID;
                    order.DeliveryDateTime = orderViewModel.DeliveryDateTime;
                    order.DelivererID = order.DelivererID ?? _currentDeliverer.DelivererID;
                    _context.SaveChanges();
                    LoadOrders();
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        public void RefreshOrdersGrid()
        {
            _ordersView?.Refresh();
        }
    }

    public class ShopViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}