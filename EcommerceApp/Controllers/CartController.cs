using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "NexHard_Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // MÉTODOS AUXILIARES (para manejar la sesión)
        // ============================================

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }

        // ============================================
        // VER CARRITO
        // ============================================
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // ============================================
        // AGREGAR AL CARRITO (AJAX)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Producto no encontrado" });

            if (product.Stock <= 0)
                return Json(new { success = false, message = "Producto agotado" });

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existing != null)
            {
                // Validar que no supere el stock
                if (existing.Quantity + quantity > product.Stock)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Solo hay {product.Stock} unidades disponibles"
                    });
                }
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Category = product.Category,
                    Price = product.Price,
                    Quantity = quantity,
                    StockDisponible = product.Stock
                });
            }

            SaveCart(cart);

            int totalItems = cart.Sum(c => c.Quantity);
            return Json(new
            {
                success = true,
                message = "Producto agregado al carrito",
                totalItems = totalItems
            });
        }

        // ============================================
        // ACTUALIZAR CANTIDAD (AJAX)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item == null)
                return Json(new { success = false, message = "Producto no está en el carrito" });

            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                if (quantity > item.StockDisponible)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Solo hay {item.StockDisponible} unidades disponibles"
                    });
                }
                item.Quantity = quantity;
            }

            SaveCart(cart);

            decimal subtotal = cart.Sum(c => c.Subtotal);
            int totalItems = cart.Sum(c => c.Quantity);
            decimal itemSubtotal = item?.Subtotal ?? 0;

            return Json(new
            {
                success = true,
                itemSubtotal = itemSubtotal,
                subtotal = subtotal,
                totalItems = totalItems
            });
        }

        // ============================================
        // ELIMINAR PRODUCTO (AJAX)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
                cart.Remove(item);

            SaveCart(cart);

            decimal subtotal = cart.Sum(c => c.Subtotal);
            int totalItems = cart.Sum(c => c.Quantity);

            return Json(new
            {
                success = true,
                subtotal = subtotal,
                totalItems = totalItems
            });
        }

        // ============================================
        // VACIAR CARRITO
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index");
        }

        // ============================================
        // OBTENER CONTADOR (para el badge del navbar)
        // ============================================
        [HttpGet]
        public IActionResult GetCount()
        {
            var cart = GetCart();
            int totalItems = cart.Sum(c => c.Quantity);
            return Json(new { count = totalItems });
        }

        // ============================================
        // OBTENER TOTAL (para el subtotal del carrito)
        // ============================================
        [HttpGet]
        public IActionResult GetTotal()
        {
            var cart = GetCart();
            decimal subtotal = cart.Sum(c => c.Subtotal);
            return Json(new { subtotal });
        }
    }
}