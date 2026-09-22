using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Número de orden único: NX-20260921-A3F8
        [Required]
        public string NumeroOrden { get; set; } = string.Empty;

        // Usuario que compra
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Datos de envío
        [Required]
        public string DireccionEnvio { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        public string? Ciudad { get; set; }
        public string? Notas { get; set; }

        // Método de pago: QR, Tarjeta, Contra-entrega
        public string MetodoPago { get; set; } = "QR";

        // Totales (en Bs)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Envio { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Estado: Pendiente, Pagado, Enviado, Entregado, Cancelado
        public string Estado { get; set; } = "Pendiente";

        // Relación 1 a N con OrderDetail
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }
}