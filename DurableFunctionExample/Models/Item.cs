using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    [Table("Item", Schema ="Tienda")]
   
    public class Item
    {
        [Key]
        public int ItemId { get; set; }
        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }
        [Required]
        public int ItemStock { get; set; }
        [Required]
        public decimal Price { get; set; }
    }
}