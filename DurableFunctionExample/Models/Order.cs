using Grpc.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    [Table("Order")]
    public  class Order
    {
        [Key]
        public int OrderId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public ICollection<ItemOrder> Items { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice => Items.Sum(item => item.Cantidad * item.Item.Price);
        public int Status { get; set; }
        public int PaymentStatus { get; set; }
    }
}