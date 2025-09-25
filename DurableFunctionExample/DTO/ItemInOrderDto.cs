

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.DTO
{
    [NotMapped]
    public class ItemInOrderDto
    {
        public string ItemName { get; set; }
        public decimal Price { get; set; }
    }
}
