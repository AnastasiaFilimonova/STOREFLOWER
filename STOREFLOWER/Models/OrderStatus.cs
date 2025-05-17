using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOREFLOWER.Models
{
    public class OrderStatus
    {
        [Key] 
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
