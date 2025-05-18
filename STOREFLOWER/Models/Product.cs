using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOREFLOWER.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public int StoreID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Навигационное свойство для связи с Stores
        public Store Store { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
