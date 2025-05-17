using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOREFLOWER.Models
{
    public class Admin
    {
        public int AdminID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public int StoreID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }

        // Навигационное свойство для связи с Stores
        public Store Store { get; set; }
    }
}
