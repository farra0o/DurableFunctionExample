using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DurableFunctionExample.Models
{
    [Table("User", Schema = "Tienda")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Correo { get; set; }
        public string? Nombre { get; set; }
    }
}