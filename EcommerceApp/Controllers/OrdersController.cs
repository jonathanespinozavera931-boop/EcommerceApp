using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================
        // MIS PEDIDOS (Vista de Cliente)
        // ============================================
        public async Task<IActionResult> MyOrders(string? estado)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _context.Orders
                .Include(o => o.Details)
                .Where(o => o.UserId == user.Id);

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(o => o.Estado == estado);
            }

            var orders = await query
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();

            ViewBag.EstadoFiltro = estado;
            return View(orders);
        }

        // ============================================
        // TODOS LOS PEDIDOS (Vista de ADMIN)
        // ============================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders(string? estado, string? buscar)
        {
            var query = _context.Orders
                .Include(o => o.Details)
                .Include(o => o.User)
                .AsQueryable();

            // Filtro por estado
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(o => o.Estado == estado);
            }

            // Filtro por búsqueda (número de orden o email del cliente)
            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(o =>
                    o.NumeroOrden.Contains(buscar) ||
                    (o.User != null && o.User.Email != null && o.User.Email.Contains(buscar)));
            }

            var orders = await query
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();

            // Estadísticas para el admin
            ViewBag.EstadoFiltro = estado;
            ViewBag.Buscar = buscar;
            ViewBag.TotalPedidos = await _context.Orders.CountAsync();
            ViewBag.TotalIngresos = await _context.Orders.SumAsync(o => o.Total);
            ViewBag.Pendientes = await _context.Orders.CountAsync(o => o.Estado == "Pendiente");
            ViewBag.Pagados = await _context.Orders.CountAsync(o => o.Estado == "Pagado");
            ViewBag.Enviados = await _context.Orders.CountAsync(o => o.Estado == "Enviado");
            ViewBag.Entregados = await _context.Orders.CountAsync(o => o.Estado == "Entregado");

            return View(orders);
        }

        // ============================================
        // ACTUALIZAR ESTADO (Solo Admin)
        // ============================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string nuevoEstado)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            string[] estadosValidos = { "Pendiente", "Pagado", "Enviado", "Entregado", "Cancelado" };
            if (!estadosValidos.Contains(nuevoEstado))
            {
                TempData["Error"] = "Estado no válido";
                return RedirectToAction("AllOrders");
            }

            order.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado actualizado a '{nuevoEstado}'";
            return RedirectToAction("Details", new { id = order.Id });
        }

        // ============================================
        // DETALLE DE ORDEN
        // ============================================
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.Details)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Seguridad: solo el dueño o un admin puede ver la orden
            bool isAdmin = User.IsInRole("Admin");
            if (order.UserId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            ViewBag.IsAdmin = isAdmin;
            return View(order);
        }
    }
}