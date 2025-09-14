using System.Security.Claims;
using EasyGames.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Web.Controllers
{
    [Authorize] // require login for both roles
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrdersController(ApplicationDbContext db) => _db = db;

        // Lightweight rows for the list page
        public class OrderRow
        {
            public int Id { get; set; }
            public string? Email { get; set; }  // filled only for Owner
            public decimal Total { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // Lightweight rows for the details page
        public class OrderItemRow
        {
            public string ProductName { get; set; } = "";
            public decimal UnitPrice { get; set; }
            public int Qty { get; set; }
            public decimal Subtotal => UnitPrice * Qty;
        }

        // GET: /Orders
        // Owner -> all orders (+ optional email filter)
        // Customer -> only own orders
        public async Task<IActionResult> Index(string? email)
        {
            bool isOwner = User.IsInRole("Owner");

            if (isOwner)
            {
                var q =
                    from o in _db.Orders
                    join u in _db.Users on o.UserId equals u.Id
                    where string.IsNullOrWhiteSpace(email) || (u.Email ?? "").Contains(email)
                    orderby o.CreatedAt descending
                    select new OrderRow
                    {
                        Id = o.Id,
                        Email = u.Email,
                        Total = o.Total,
                        CreatedAt = o.CreatedAt
                    };

                ViewBag.Email = email ?? "";
                return View(await q.ToListAsync());
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var list = await _db.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OrderRow
                    {
                        Id = o.Id,
                        Total = o.Total,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                return View(list);
            }
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            // Customers can only view their own orders
            bool isOwner = User.IsInRole("Owner");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (!isOwner && order.UserId != userId) return Forbid();

            // Load items with product names
            var items =
                await (from i in _db.OrderItems
                       join p in _db.Products on i.ProductId equals p.Id
                       where i.OrderId == id
                       select new OrderItemRow
                       {
                           ProductName = p.Name,
                           UnitPrice = i.UnitPrice,
                           Qty = i.Qty
                       }).ToListAsync();

            ViewBag.OrderId = order.Id;
            ViewBag.Total = order.Total;
            ViewBag.CreatedAt = order.CreatedAt;
            ViewBag.CustomerEmail = isOwner
                ? await _db.Users.Where(u => u.Id == order.UserId).Select(u => u.Email).FirstOrDefaultAsync()
                : null;

            return View(items);
        }
    }
}
