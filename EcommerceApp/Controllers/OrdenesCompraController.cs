
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Models;
using EcommerceApp.Data;

public class OrdenesCompraController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdenesCompraController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ORDENCOMPRAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.OrdenesCompra.ToListAsync());
    }

    // GET: ORDENCOMPRAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ordencompra = await _context.OrdenesCompra
            .FirstOrDefaultAsync(m => m.Id == id);
        if (ordencompra == null)
        {
            return NotFound();
        }

        return View(ordencompra);
    }

    // GET: ORDENCOMPRAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ORDENCOMPRAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Fecha,ClienteId,Cliente,ProductoId,Producto,Cantidad,Total,Estado")] OrdenCompra ordencompra)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ordencompra);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ordencompra);
    }

    // GET: ORDENCOMPRAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ordencompra = await _context.OrdenesCompra.FindAsync(id);
        if (ordencompra == null)
        {
            return NotFound();
        }
        return View(ordencompra);
    }

    // POST: ORDENCOMPRAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Fecha,ClienteId,Cliente,ProductoId,Producto,Cantidad,Total,Estado")] OrdenCompra ordencompra)
    {
        if (id != ordencompra.Id)
        {
            return NotFound();
        }

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
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(ordencompra);
    }

    // GET: ORDENCOMPRAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ordencompra = await _context.OrdenesCompra
            .FirstOrDefaultAsync(m => m.Id == id);
        if (ordencompra == null)
        {
            return NotFound();
        }

        return View(ordencompra);
    }

    // POST: ORDENCOMPRAS/Delete/5
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
