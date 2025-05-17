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
                .Where(o => o.StoreID == _currentAdmin.StoreID)
                .ToList();

            _allOrders = orders
                .Where(o => o.StatusID == 1 || o.StatusID == 2)
                .Select(o => new OrderViewModel
                {
                    OrderNumber = o.OrderID.ToString(),
                    CreationDate = o.OrderDate.ToString("dd.MM.yyyy"),
                    Status = o.Status?.StatusName ?? "Не указан",
                    StatusID = o.StatusID,
                    Customer = $"{o.ClientLastName} {o.ClientFirstName} {o.ClientPatronymic}",
                    ClientPhone = o.ClientPhoneNumber, // Добавлен номер телефона клиента
                    FloristDisplay = o.Florist != null ? $"{o.Florist.LastName} {o.Florist.FirstName} {o.Florist.Patronymic}" : "Не назначен",
                    FloristId = o.Florist?.FloristID ?? 0,
                    AvailableFlorists = _context.Florists
                        .Where(f => f.StoreID == _currentAdmin.StoreID)
                        .Select(f => new FloristViewModel
                        {
                            FullName = $"{f.LastName} {f.FirstName} {f.Patronymic}",
                            Id = f.FloristID
                        })
                        .ToList()
                })
                .ToList();

            OrdersGrid.ItemsSource = _allOrders;
            OrdersGrid.CellEditEnding += OrdersGrid_CellEditEnding;
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

        private void OrdersGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Флорист")
            {
                var order = e.Row.DataContext as OrderViewModel;
                if (order != null)
                {
                    var comboBox = e.EditingElement as ComboBox;
                    if (comboBox != null)
                    {
                        var selectedFlorist = comboBox.SelectedItem as FloristViewModel;
                        if (selectedFlorist != null)
                        {
                            order.FloristDisplay = selectedFlorist.FullName;
                            order.FloristId = selectedFlorist.Id;

                            // Обновляем заказ в базе данных
                            var dbOrder = _context.Orders.FirstOrDefault(o => o.OrderID == int.Parse(order.OrderNumber));
                            if (dbOrder != null)
                            {
                                dbOrder.FloristID = selectedFlorist.Id;
                                _context.SaveChanges();
                            }
                        }
                        else
                        {
                            // Если флорист не выбран (например, сброшен), сбрасываем назначение
                            order.FloristDisplay = "Не назначен";
                            order.FloristId = 0;

                            var dbOrder = _context.Orders.FirstOrDefault(o => o.OrderID == int.Parse(order.OrderNumber));
                            if (dbOrder != null)
                            {
                                dbOrder.FloristID = null;
                                _context.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOrderWindow createOrderWindow = new CreateOrderWindow();
            createOrderWindow.Show();
            this.Close();
        }
    }

    public class OrderViewModel
    {
        public string OrderNumber { get; set; }
        public string CreationDate { get; set; }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public string Customer { get; set; }
        public string ClientPhone { get; set; } // Добавлен номер телефона клиента
        public string FloristDisplay { get; set; }
        public int FloristId { get; set; }
        public List<FloristViewModel> AvailableFlorists { get; set; }
        public FloristViewModel SelectedFlorist { get; set; }
    }

    public class FloristViewModel
    {
        public string FullName { get; set; } // Теперь полное имя с отчеством
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Id { get; set; }
    }
}