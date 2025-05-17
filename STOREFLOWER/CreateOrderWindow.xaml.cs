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
    /// Логика взаимодействия для CreateOrderWindow.xaml
    /// </summary>
    public partial class CreateOrderWindow : Window
    {
        private StoreFlowerContext _context;

        public CreateOrderWindow()
        {
            InitializeComponent();
            _context = new StoreFlowerContext();
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(CustomerPhoneTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DeliveryAddressTextBox.Text) ||
                    StoreAddressComboBox.Text == null ||
                    DeliveryDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Разделим ФИО
                var fullName = CustomerNameTextBox.Text.Trim().Split(' ');
                if (fullName.Length < 3)
                {
                    MessageBox.Show("ФИО должно содержать фамилию, имя и отчество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string lastName = fullName[0];
                string firstName = fullName[1];
                string patronymic = fullName[2];

                // Поиск StoreID по адресу магазина (если Store уже есть в БД)
                int storeId = _context.Stores
                    .FirstOrDefault(s => s.Address == StoreAddressComboBox.Text)?.StoreID ?? 0;

                if (storeId == 0)
                {
                    MessageBox.Show("Магазин с таким адресом не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    DeliveryAddress = DeliveryAddressTextBox.Text.Trim(),
                    ClientPhoneNumber = CustomerPhoneTextBox.Text.Trim(),
                    StoreID = storeId,
                    DeliveryDateTime = DeliveryDatePicker.SelectedDate.Value,
                    StatusID = 1, // Присвоим "новый заказ", если 1 — это статус "новый"
                    FloristID = null,
                    DelivererID = 1, // Можно временно задать вручную или через другой выбор
                    ClientLastName = lastName,
                    ClientFirstName = firstName,
                    ClientPatronymic = patronymic
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                MessageBox.Show("Заказ успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

