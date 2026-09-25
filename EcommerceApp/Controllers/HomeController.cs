using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EcommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Contar productos por categoría
            var categorias = new Dictionary<string, int>
            {
                { "CPU", await _context.Products.CountAsync(p => p.Category == "CPU") },
                { "GPU", await _context.Products.CountAsync(p => p.Category == "GPU") },
                { "RAM", await _context.Products.CountAsync(p => p.Category == "RAM") },
                { "Almacenamiento", await _context.Products.CountAsync(p => p.Category == "Almacenamiento") },
                { "Placa Madre", await _context.Products.CountAsync(p => p.Category == "Placa Madre") },
                { "Fuente", await _context.Products.CountAsync(p => p.Category == "Fuente") },
                { "Refrigeración", await _context.Products.CountAsync(p => p.Category == "Refrigeración") },
                { "Gabinete", await _context.Products.CountAsync(p => p.Category == "Gabinete") },
                { "Periférico", await _context.Products.CountAsync(p => p.Category == "Periférico") },
                { "Monitor", await _context.Products.CountAsync(p => p.Category == "Monitor") },
            };

            ViewBag.Categorias = categorias;

            // Producto destacado grande (el más caro, o uno específico)
            var productoDestacado = await _context.Products
                .Where(p => p.Stock > 0)
                .OrderByDescending(p => p.Price)
                .FirstOrDefaultAsync();

            ViewBag.ProductoDestacado = productoDestacado;

            // 3 productos mini (los más recientes o aleatorios)
            var productosMini = await _context.Products
                .Where(p => p.Stock > 0)
                .OrderByDescending(p => p.Id)
                .Skip(1)
                .Take(3)
                .ToListAsync();

            ViewBag.ProductosMini = productosMini;

            // Productos destacados para la grilla de tabs (los 8 más caros)
            var productosDestacados = await _context.Products
                .Where(p => p.Stock > 0)
                .OrderByDescending(p => p.Price)
                .Take(8)
                .ToListAsync();

            ViewBag.ProductosDestacados = productosDestacados;

            // Productos para "Equipa tu Setup"
            var productosSetup = await _context.Products
                .Where(p => p.Stock > 0 && (p.Category == "Periférico" || p.Category == "Monitor"))
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            ViewBag.ProductosSetup = productosSetup;

            // Producto para banner 4K (una GPU)
            var gpuBanner = await _context.Products
                .Where(p => p.Category == "GPU" && p.Stock > 0)
                .OrderByDescending(p => p.Price)
                .FirstOrDefaultAsync();

            ViewBag.GpuBanner = gpuBanner;

            return View();
        }

        public IActionResult Privacy() => View();

        public IActionResult Contact() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}