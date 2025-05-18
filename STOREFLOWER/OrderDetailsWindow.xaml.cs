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
    /// Логика взаимодействия для OrderDetailsWindow.xaml
    /// </summary>
    public partial class OrderDetailsWindow : Window
    {
        public OrderDetailsWindow(OrderViewModel order)
        {
            InitializeComponent();
            if (order == null)
            {
                MessageBox.Show("Заказ не передан в окно деталей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                MessageBox.Show("Состав заказа пуст.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            DataContext = order;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}