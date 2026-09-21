using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class OrdenCompra
    {
        [Key]
        public int Id { get; set; }

        // ⬇️ CAMBIA DateTime.Now por DateTime.UtcNow
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Product? Producto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente";
    }
}