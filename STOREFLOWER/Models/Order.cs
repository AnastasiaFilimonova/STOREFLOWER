using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOREFLOWER.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string ClientPhoneNumber { get; set; }
        public int StoreID { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public int StatusID { get; set; }
        public int? FloristID { get; set; }
        public int DelivererID { get; set; }
        public string ClientLastName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientPatronymic { get; set; }

        // Навигационные свойства для связей
        public Store Store { get; set; }
        public OrderStatus Status { get; set; }
        public Florist Florist { get; set; }
        public Deliverer Deliverer { get; set; }
    }
}
