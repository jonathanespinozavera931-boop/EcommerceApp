using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class OrdenCompra
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Relación con Cliente
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        // Relación con Producto (Componente)
        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Product? Producto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente"; // Ej: Pendiente, Pagado, Enviado
    }
}