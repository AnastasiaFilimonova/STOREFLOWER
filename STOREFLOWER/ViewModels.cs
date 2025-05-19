using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOREFLOWER
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _orderNumber;
        private DateTime _creationDateTime;
        private string _creationDate;
        private string _deliveryAddress;
        private DateTime? _deliveryDateTime;
        private string _status;
        private int _statusID;
        private string _customer;
        private string _clientPhone;
        private string _floristDisplay;
        private string _delivererDisplay;
        private int? _floristId;
        private List<FloristViewModel> _availableFlorists;
        private List<OrderStatusViewModel> _availableStatuses;
        private List<OrderItemViewModel> _orderItems;
        private int? _storeID;

        public string OrderNumber
        {
            get => _orderNumber;
            set { _orderNumber = value; OnPropertyChanged(nameof(OrderNumber)); }
        }

        public DateTime CreationDateTime
        {
            get => _creationDateTime;
            set { _creationDateTime = value; OnPropertyChanged(nameof(CreationDateTime)); }
        }

        public string CreationDate
        {
            get => _creationDate;
            set { _creationDate = value; OnPropertyChanged(nameof(CreationDate)); }
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set { _deliveryAddress = value; OnPropertyChanged(nameof(DeliveryAddress)); }
        }

        public DateTime? DeliveryDateTime
        {
            get => _deliveryDateTime;
            set { _deliveryDateTime = value; OnPropertyChanged(nameof(DeliveryDateTime)); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public int StatusID
        {
            get => _statusID;
            set { _statusID = value; OnPropertyChanged(nameof(StatusID)); }
        }

        public string Customer
        {
            get => _customer;
            set { _customer = value; OnPropertyChanged(nameof(Customer)); }
        }

        public string ClientPhone
        {
            get => _clientPhone;
            set { _clientPhone = value; OnPropertyChanged(nameof(ClientPhone)); }
        }

        public string FloristDisplay
        {
            get => _floristDisplay;
            set { _floristDisplay = value; OnPropertyChanged(nameof(FloristDisplay)); }
        }

        public string DelivererDisplay
        {
            get => _delivererDisplay;
            set { _delivererDisplay = value; OnPropertyChanged(nameof(DelivererDisplay)); }
        }

        public int? FloristId
        {
            get => _floristId;
            set { _floristId = value; OnPropertyChanged(nameof(FloristId)); }
        }

        public List<FloristViewModel> AvailableFlorists
        {
            get => _availableFlorists;
            set { _availableFlorists = value; OnPropertyChanged(nameof(AvailableFlorists)); }
        }

        public List<OrderStatusViewModel> AvailableStatuses
        {
            get => _availableStatuses;
            set { _availableStatuses = value; OnPropertyChanged(nameof(AvailableStatuses)); }
        }

        public List<OrderItemViewModel> OrderItems
        {
            get => _orderItems;
            set { _orderItems = value; OnPropertyChanged(nameof(OrderItems)); }
        }

        public int? StoreID
        {
            get => _storeID;
            set { _storeID = value; OnPropertyChanged(nameof(StoreID)); }
        }

        public decimal TotalOrderPrice => OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;
    }

    public class FloristViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Id { get; set; }
    }

    public class CourierViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class OrderStatusViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }
}