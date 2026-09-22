using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        // Orden a la que pertenece
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        // Producto comprado
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // Guardamos el nombre e imagen al momento de la compra
        // (por si luego cambia el precio o el nombre del producto)
        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}