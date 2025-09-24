using Grpc.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    [Table("Orders")]
    public  class Order
    {
        [Key]
        public int OrderId { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }

        // FK
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
         

        // Relaciones
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}