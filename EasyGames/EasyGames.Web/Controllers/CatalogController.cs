using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Web.Data;

namespace EasyGames.Web.Controllers
{
    // Public read-only catalog
    [AllowAnonymous]
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CatalogController(ApplicationDbContext db) => _db = db;

        // GET: /Catalog?category=Book&q=chess
        public async Task<IActionResult> Index(string? category, string? q)
        {
            // Build base query
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q));

            // Data for the filter UI
            ViewBag.Categories = await _db.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.SelectedCategory = category;
            ViewBag.Q = q;

            var products = await query
                .OrderByDescending(p => p.CreatedAt) // newest first
                .ToListAsync();

            return View(products);
        }

        // GET: /Catalog/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
