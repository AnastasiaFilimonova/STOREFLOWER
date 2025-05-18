using STOREFLOWER.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STOREFLOWER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StoreFlowerContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new StoreFlowerContext();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginTextBox == null || PasswordTextBox == null)
                {
                    MessageBox.Show("Ошибка инициализации полей ввода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string login = LoginTextBox.Text?.Trim() ?? "";
                string password = PasswordTextBox.Password?.Trim() ?? "";

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка учетных данных для каждой роли
                var admin = _context.Admins.FirstOrDefault(a => a.Login == login && a.Password == password);
                var florist = _context.Florists.FirstOrDefault(f => f.Login == login && f.Password == password);
                var deliverer = _context.Deliverers.FirstOrDefault(d => d.Login == login && d.Password == password);

                if (admin != null)
                {
                    // Передаем объект admin в AdminWindow
                    AdminWindow adminWindow = new AdminWindow(admin);
                    adminWindow.Show();
                    this.Close();
                }
                else if (florist != null)
                {
                    FloristWindow floristWindow = new FloristWindow(florist);
                    floristWindow.Show();
                    this.Close();
                }
                else if (deliverer != null)
                {
                    DeliverWindow deliverWindow = new DeliverWindow();
                    deliverWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
