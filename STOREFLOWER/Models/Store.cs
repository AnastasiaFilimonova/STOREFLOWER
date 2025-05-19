using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STOREFLOWER.Models
{
    public class Store
    {
        public int StoreID { get; set; }
        public string Address { get; set; }
        public ICollection<Admin> Admins { get; set; }
        public ICollection<Florist> Florists { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
