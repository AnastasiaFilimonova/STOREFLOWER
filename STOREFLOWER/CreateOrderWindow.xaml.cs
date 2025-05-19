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
    /// Логика взаимодействия для CreateOrderWindow.xaml
    /// </summary>
    public partial class CreateOrderWindow : Window
    {
        private readonly StoreFlowerContext _context;
        private readonly Admin _currentAdmin;
        private List<CreateOrderItemViewModel> _orderItems = new List<CreateOrderItemViewModel>();
        private List<Product> _availableProducts;
        public CreateOrderWindow(Admin admin)
        {
            InitializeComponent();
            _context = new StoreFlowerContext();
            _currentAdmin = admin;
            LoadStores();
            LoadProducts();
            StoreAddressComboBox.SelectedValue = _currentAdmin.StoreID;
        }

        private void LoadStores()
        {
            var stores = _context.Stores.ToList();
            StoreAddressComboBox.ItemsSource = stores;
        }

        private void LoadProducts()
        {
            _availableProducts = _context.Products
                .Where(p => p.StoreID == _currentAdmin.StoreID)
                .ToList();
            OrderItemsListBox.ItemsSource = _orderItems;

            if (!_availableProducts.Any())
            {
                MessageBox.Show("В текущем магазине нет доступных продуктов. Добавьте продукты перед созданием заказа.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddOrderItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_availableProducts.Any())
            {
                MessageBox.Show("Нет доступных продуктов для добавления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newItem = new CreateOrderItemViewModel
            {
                AvailableProducts = _availableProducts,
                ProductID = _availableProducts.First().ProductID,
                Price = _availableProducts.First().Price,
                Quantity = 1
            };
            _orderItems.Add(newItem);
            OrderItemsListBox.Items.Refresh();
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is CreateOrderItemViewModel item)
            {
                item.ProductID = (int)(comboBox.SelectedValue ?? 0);
                item.Price = _availableProducts.FirstOrDefault(p => p.ProductID == item.ProductID)?.Price ?? 0m;
            }
        }

        private void RemoveOrderItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CreateOrderItemViewModel item)
            {
                _orderItems.Remove(item);
                OrderItemsListBox.Items.Refresh();
            }
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(CustomerPhoneTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DeliveryAddressTextBox.Text) ||
                    StoreAddressComboBox.SelectedValue == null ||
                    DeliveryDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var fullName = CustomerNameTextBox.Text.Trim().Split(' ');
                if (fullName.Length < 3)
                {
                    MessageBox.Show("Введите ФИО полностью (Фамилия Имя Отчество)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!_orderItems.Any())
                {
                    MessageBox.Show("Добавьте хотя бы один продукт в состав заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                foreach (var item in _orderItems)
                {
                    if (item.Quantity <= 0)
                    {
                        MessageBox.Show("Количество каждого продукта должно быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var product = _availableProducts.FirstOrDefault(p => p.ProductID == item.ProductID);
                    if (product != null && product.Stock < item.Quantity)
                    {
                        MessageBox.Show($"Недостаточно товара '{product.ProductName}' на складе. Доступно: {product.Stock}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                var newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    DeliveryAddress = DeliveryAddressTextBox.Text.Trim(),
                    ClientPhoneNumber = CustomerPhoneTextBox.Text.Trim(),
                    StoreID = (int)StoreAddressComboBox.SelectedValue,
                    DeliveryDateTime = DeliveryDatePicker.SelectedDate.Value,
                    StatusID = 1, 
                    FloristID = null,
                    DelivererID = null,
                    ClientLastName = fullName[0],
                    ClientFirstName = fullName[1],
                    ClientPatronymic = fullName[2],
                    OrderItems = _orderItems.Select(oi => new OrderItem
                    {
                        ProductID = oi.ProductID,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                };
                _context.Orders.Add(newOrder);
                _context.SaveChanges();
                foreach (var item in _orderItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductID == item.ProductID);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                    }
                }
                _context.SaveChanges();

                MessageBox.Show("Заказ успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                var adminWindow = new AdminWindow(_currentAdmin);
                adminWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow(_currentAdmin);
            adminWindow.Show();
            this.Close();
        }
    }

    public class CreateOrderItemViewModel : INotifyPropertyChanged
    {
        private int _quantity = 1;
        private int _productID;
        private decimal _price;

        public List<Product> AvailableProducts { get; set; }

        public int ProductID
        {
            get => _productID;
            set
            {
                _productID = value;
                OnPropertyChanged(nameof(ProductID));
                OnPropertyChanged(nameof(ProductName));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        public string ProductName => AvailableProducts?.FirstOrDefault(p => p.ProductID == ProductID)?.ProductName ?? "Не указан";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
