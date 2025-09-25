using DurableFunctionExample.DTO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    [Table("ItemOrder", Schema = "Tienda")]
    public class ItemOrder
    {
        [Key]
        public int ItemOrderId { get; set; }
        [Required]
        public int Cantidad { get; set; }
        [Required]
        public ItemInOrderDto Item { get; set; }
    }
}