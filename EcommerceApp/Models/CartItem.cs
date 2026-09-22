namespace EcommerceApp.Models
{
    /// <summary>
    /// Representa un producto dentro del carrito de compras.
    /// NO se guarda en base de datos, vive en la sesión del usuario.
    /// </summary>
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockDisponible { get; set; }

        // Calculado automáticamente: Price * Quantity
        public decimal Subtotal => Price * Quantity;
    }
}