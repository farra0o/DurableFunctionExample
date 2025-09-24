using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    public class PaymentResult
    {
        [Key]
        
        public int PaymentId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        [DataType(DataType.Date)]
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

    }
}