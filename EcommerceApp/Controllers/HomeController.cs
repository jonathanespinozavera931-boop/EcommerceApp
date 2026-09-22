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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}