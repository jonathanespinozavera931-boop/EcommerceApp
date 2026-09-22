using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceApp.Controllers
{
    [Authorize] // Solo usuarios logueados pueden comprar
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartSessionKey = "NexHard_Cart";

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        // GET: /Checkout
        public IActionResult Index()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }
            return View();
        }

        // POST: /Checkout/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            string direccionEnvio,
            string telefono,
            string? ciudad,
            string metodoPago,
            string? notas)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(direccionEnvio) || string.IsNullOrWhiteSpace(telefono))
            {
                TempData["Error"] = "Debes completar la dirección y el teléfono";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Calcular totales
            decimal subtotal = cart.Sum(c => c.Subtotal);
            decimal envio = subtotal >= 500 ? 0 : 20;
            decimal total = subtotal + envio;

            // Crear la orden
            var order = new Order
            {
                NumeroOrden = GenerarNumeroOrden(),
                UserId = user.Id,
                Fecha = DateTime.UtcNow,
                DireccionEnvio = direccionEnvio,
                Telefono = telefono,
                Ciudad = ciudad,
                MetodoPago = metodoPago,
                Notas = notas,
                Subtotal = subtotal,
                Envio = envio,
                Descuento = 0,
                Total = total,
                Estado = "Pendiente"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Crear los detalles
            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    ProductImage = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Subtotal = item.Subtotal
                };
                _context.OrderDetails.Add(detail);

                // Descontar stock
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    if (product.Stock < 0) product.Stock = 0;
                }
            }

            await _context.SaveChangesAsync();

            // Vaciar el carrito
            HttpContext.Session.Remove(CartSessionKey);

            // Redirigir a la página de éxito
            return RedirectToAction("Success", new { id = order.Id });
        }

        // GET: /Checkout/Success/5
        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // Genera un número de orden único: NX-20260921-A3F8
        private string GenerarNumeroOrden()
        {
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            string random = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            return $"NX-{fecha}-{random}";
        }
    }
}