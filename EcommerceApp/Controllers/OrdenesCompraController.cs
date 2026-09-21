using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdenesCompraController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdenesCompraController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrdenesCompra
        public async Task<IActionResult> Index()
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.Cliente)
                .Include(o => o.Producto)
                .ToListAsync();
            return View(ordenes);
        }

        // GET: OrdenesCompra/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ordencompra = await _context.OrdenesCompra
                .Include(o => o.Cliente)
                .Include(o => o.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordencompra == null) return NotFound();

            return View(ordencompra);
        }

        // GET: OrdenesCompra/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["ProductoId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: OrdenesCompra/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,ClienteId,ProductoId,Cantidad,Total,Estado")] OrdenCompra ordencompra)
        {
            // ⬇️ ESTA ES LA LÍNEA QUE ARREGLA EL ERROR DE LA FECHA
            ordencompra.Fecha = DateTime.SpecifyKind(ordencompra.Fecha, DateTimeKind.Utc);

            if (ModelState.IsValid)
            {
                _context.Add(ordencompra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Clientes, "Id", "Nombre", ordencompra.ClienteId);
            ViewData["ProductoId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Products, "Id", "Name", ordencompra.ProductoId);
            return View(ordencompra);
        }

        // GET: OrdenesCompra/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ordencompra = await _context.OrdenesCompra.FindAsync(id);
            if (ordencompra == null) return NotFound();

            ViewData["ClienteId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Clientes, "Id", "Nombre", ordencompra.ClienteId);
            ViewData["ProductoId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Products, "Id", "Name", ordencompra.ProductoId);
            return View(ordencompra);
        }

        // POST: OrdenesCompra/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Fecha,ClienteId,ProductoId,Cantidad,Total,Estado")] OrdenCompra ordencompra)
        {
            if (id != ordencompra.Id) return NotFound();

            // ⬇️ TAMBIÉN AGREGA LA MISMA LÍNEA AQUÍ (para cuando edites una orden)
            ordencompra.Fecha = DateTime.SpecifyKind(ordencompra.Fecha, DateTimeKind.Utc);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordencompra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenCompraExists(ordencompra.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Clientes, "Id", "Nombre", ordencompra.ClienteId);
            ViewData["ProductoId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Products, "Id", "Name", ordencompra.ProductoId);
            return View(ordencompra);
        }

        // GET: OrdenesCompra/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ordencompra = await _context.OrdenesCompra
                .Include(o => o.Cliente)
                .Include(o => o.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordencompra == null) return NotFound();

            return View(ordencompra);
        }

        // POST: OrdenesCompra/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var ordencompra = await _context.OrdenesCompra.FindAsync(id);
            if (ordencompra != null)
            {
                _context.OrdenesCompra.Remove(ordencompra);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenCompraExists(int? id)
        {
            return _context.OrdenesCompra.Any(e => e.Id == id);
        }
    }
}