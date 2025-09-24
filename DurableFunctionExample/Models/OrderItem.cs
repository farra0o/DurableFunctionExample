using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int Quantity { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        // FK
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
