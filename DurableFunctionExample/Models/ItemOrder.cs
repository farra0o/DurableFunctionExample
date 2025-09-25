using DurableFunctionExample.DTO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    [Table("ItemOrder")]
    public class ItemOrder
    {
        [Key]
        public int ItemOrderId { get; set; }
        public int Cantidad { get; set; }
        public ItemInOrderDto Item { get; set; }
    }
}