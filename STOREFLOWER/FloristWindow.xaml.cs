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
    /// Логика взаимодействия для FloristWindow.xaml
    /// </summary>
    public partial class FloristWindow : Window
    {
        private readonly StoreFlowerContext _context;
        private readonly Florist _currentFlorist;
        private List<OrderViewModel> _allOrders;
        private ListCollectionView _ordersView; 
        public FloristWindow(Florist florist)
        {
            InitializeComponent();
            _context = new StoreFlowerContext();
            _currentFlorist = florist;
            LoadFloristData();
            LoadCouriers();
            LoadOrders();
        }

        private void LoadFloristData()
        {
            FloristNameTextBlock.Text = $"{_currentFlorist.LastName} {_currentFlorist.FirstName} {_currentFlorist.Patronymic}";
            var store = _context.Stores.FirstOrDefault(s => s.StoreID == _currentFlorist.StoreID);
            StoreAddressTextBlock.Text = store?.Address ?? "Магазин не найден";
        }

        private void LoadCouriers()
        {
            var couriers = _context.Deliverers
                .ToList()
                .Select(d => new CourierViewModel
                {
                    FullName = $"{d.LastName} {d.FirstName} {d.Patronymic}",
                    Phone = d.PhoneNumber
                })
                .ToList();

            CourierListBox.ItemsSource = couriers;
        }

        private string GetStoreAddress(int storeId)
        {
            return _context.Stores.FirstOrDefault(s => s.StoreID == storeId)?.Address ?? "Не указан";
        }

        private void LoadOrders()
        {
            var orders = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.FloristID == _currentFlorist.FloristID)
                .ToList();

            var availableStatuses = _context.OrderStatuses
                .Where(s => s.StatusID == 2 || s.StatusID == 3)
                .Select(s => new OrderStatusViewModel
                {
                    Id = s.StatusID,
                    Name = s.StatusName
                })
                .ToList();

            _allOrders = orders
                .Where(o => o.StatusID == 2 || o.StatusID == 3)
                .Select(o => new OrderViewModel
                {
                    OrderNumber = o.OrderID.ToString(),
                    CreationDate = o.OrderDate.ToString("dd.MM.yyyy"),
                    Status = o.Status?.StatusName ?? "Не указан",
                    StatusID = o.StatusID,
                    Customer = $"{o.ClientLastName} {o.ClientFirstName} {o.ClientPatronymic}",
                    AvailableStatuses = availableStatuses,
                    OrderItems = o.OrderItems?.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product?.ProductName ?? "Не указан",
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList() ?? new List<OrderItemViewModel>()
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
            else if (selectedFilter == "Оплачены")
            {
                _ordersView.Filter = (obj) => (obj as OrderViewModel)?.StatusID == 2;
            }
            else if (selectedFilter == "Готовы к доставке")
            {
                _ordersView.Filter = (obj) => (obj as OrderViewModel)?.StatusID == 3;
            }

            _ordersView.Refresh();
        }

        private void OrdersGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var currentFilter = _ordersView.Filter;
            _ordersView.Filter = currentFilter;
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

            var editWindow = new EditFloristOrderWindow(order, _context, _allOrders, this);
            editWindow.Owner = this;
            editWindow.ShowDialog();
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
}