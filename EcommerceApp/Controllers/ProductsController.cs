using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    public class ProductsController(ApplicationDbContext context) : Controller
    {
        // ============================================
        // INDEX - Lista con filtros y búsqueda
        // ============================================
        public async Task<IActionResult> Index(string? category, string? search, string? marca)
        {
            var query = context.Products.AsNoTracking();

            // Filtro por categoría
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Filtro por marca
            if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(p => p.Name.Contains(marca));
            }

            // Filtro por búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    p.Category.ToLower().Contains(term)
                );
            }

            var products = await query
                .OrderByDescending(p => p.Price)
                .ToListAsync();

            // Datos para la vista
            ViewBag.CategoriaActual = category;
            ViewBag.BusquedaActual = search;
            ViewBag.MarcaActual = marca;

            ViewBag.Categorias = await context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var conteos = await context.Products
                .GroupBy(p => p.Category)
                .Select(g => new { Categoria = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.ConteoPorCategoria = conteos.ToDictionary(c => c.Categoria, c => c.Count);

            // Conteo de marcas (para mostrar cantidad)
            var marcas = new[] { "AMD", "Intel", "NVIDIA", "Corsair", "ASUS", "MSI", "Gigabyte", "Kingston", "Samsung" };
            var conteoMarcas = new Dictionary<string, int>();
            foreach (var m in marcas)
            {
                conteoMarcas[m] = await context.Products.CountAsync(p => p.Name.Contains(m));
            }
            ViewBag.ConteoMarcas = conteoMarcas;

            return View(products);
        }

        // ============================================
        // DETAILS
        // ============================================
        public async Task<IActionResult> Details(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // ============================================
        // CREATE / EDIT / DELETE
        // ============================================
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();
            if (!ModelState.IsValid) return View(product);

            product.UpdatedAt = DateTime.UtcNow;
            context.Products.Update(product);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}