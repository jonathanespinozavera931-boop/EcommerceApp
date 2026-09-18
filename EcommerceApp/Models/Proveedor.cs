using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
    }
}