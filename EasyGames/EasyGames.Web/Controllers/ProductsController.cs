using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Web.Data;
using EasyGames.Web.Models;

namespace EasyGames.Web.Controllers
{
    // Only Owner can manage stock (CRUD)
    [Authorize(Roles = "Owner")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var list = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        // GET: Products/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create() => View();

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Category,Price,StockQty")] Product product)
        {
            // Bind only safe fields; server sets CreatedAt
            if (!ModelState.IsValid) return View(product);

            product.CreatedAt = DateTime.UtcNow; // server-side timestamp
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Category,Price,StockQty")] Product input)
        {
            // Do not allow editing CreatedAt via model binding
            if (id != input.Id) return NotFound();
            if (!ModelState.IsValid) return View(input);

            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return NotFound();

            // Update only allowed fields (keep CreatedAt untouched)
            entity.Name = input.Name;
            entity.Category = input.Category;
            entity.Price = input.Price;
            entity.StockQty = input.StockQty;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id) =>
            _context.Products.Any(e => e.Id == id);
    }
}
