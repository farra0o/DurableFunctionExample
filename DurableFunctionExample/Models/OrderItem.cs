using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionExample.Models
{
    internal class OrderItem
    {
        public string productID { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }

    }

}
