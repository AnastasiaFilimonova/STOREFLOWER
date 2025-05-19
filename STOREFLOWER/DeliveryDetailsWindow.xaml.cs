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
    /// Логика взаимодействия для DeliveryDetailsWindow.xaml
    /// </summary>
    public partial class DeliveryDetailsWindow : Window
    {
        private readonly OrderViewModel _order;
        private readonly StoreFlowerContext _context;
        public bool IsStatusChanged { get; private set; }
        public DeliveryDetailsWindow(OrderViewModel order, StoreFlowerContext context)
        {
            InitializeComponent();
            _order = order;
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            ClientAddressText.Text = _order.DeliveryAddress;
            var store = _context.Stores.FirstOrDefault(s => s.StoreID == _order.StoreID);
            ShopAddressText.Text = store?.Address ?? "Не указан";
            StatusComboBox.SelectedValue = _order.StatusID.ToString();
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem item)
            {
                _order.StatusID = int.Parse(item.Tag.ToString());
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            IsStatusChanged = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsStatusChanged = false;
            this.Close();
        }
    }
}
