using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EasyGames.Web.Data;
using EasyGames.Web.Helpers;      // uses SessionExtensions
using EasyGames.Web.Models;       // CartItem, Order, OrderItem
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Web.Controllers
{
    // Public cart: add/update without login; checkout requires Customer role
    public class CartController : Controller
    {
        private const string CartKey = "CART";
        private readonly ApplicationDbContext _db;
        public CartController(ApplicationDbContext db) => _db = db;

        // --- Helpers: role guards (Owner) ---
        private bool IsOwner() =>
            User.Identity?.IsAuthenticated == true && User.IsInRole("Owner");

        private IActionResult BlockOwner()
        {
            // Show a friendly message then send Owner back to Catalog
            TempData["CartError"] = "Admins/Owners cannot purchase.";
            return RedirectToAction("Index", "Catalog");
        }

        // --- Session helpers ---
        private List<CartItem> GetCart() =>
            HttpContext.Session.GetJson<List<CartItem>>(CartKey) ?? new List<CartItem>();

        private void SaveCart(List<CartItem> cart) =>
            HttpContext.Session.SetJson(CartKey, cart);

        // GET: /Cart
        public IActionResult Index()
        {
            // Deny Owner from using cart UI
            if (IsOwner()) return BlockOwner();

            var cart = GetCart();
            ViewBag.Total = cart.Sum(i => i.UnitPrice * i.Qty);
            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id, int qty = 1)
        {
            // Deny Owner from adding to cart
            if (IsOwner()) return BlockOwner();

            if (qty <= 0) qty = 1;

            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();

            var cart = GetCart();
            var line = cart.FirstOrDefault(i => i.ProductId == id);
            if (line is null)
                cart.Add(new CartItem { ProductId = id, Name = p.Name, UnitPrice = p.Price, Qty = qty });
            else
                line.Qty += qty;

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQty(int id, int qty)
        {
            // Deny Owner from modifying cart
            if (IsOwner()) return BlockOwner();

            var cart = GetCart();
            var line = cart.FirstOrDefault(i => i.ProductId == id);
            if (line != null)
            {
                if (qty <= 0) cart.Remove(line);
                else line.Qty = qty;
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            // Deny Owner from modifying cart
            if (IsOwner()) return BlockOwner();

            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == id);
            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout (login required & must be Customer)
        [Authorize(Roles = "Customer")]
        public IActionResult Checkout()
        {
            // Double-safety: if somehow Owner hits this, block
            if (IsOwner()) return BlockOwner();

            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));
            ViewBag.Total = cart.Sum(i => i.UnitPrice * i.Qty);
            return View(cart);
        }

        // POST: /Cart/CheckoutConfirm (Customer only)
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutConfirm()
        {
            // Double-safety: if somehow Owner hits this, block
            if (IsOwner()) return BlockOwner();

            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            // Validate stock for all items
            var ids = cart.Select(c => c.ProductId).ToList();
            var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
            foreach (var line in cart)
            {
                var p = products.First(x => x.Id == line.ProductId);
                if (line.Qty > p.StockQty)
                {
                    TempData["CartError"] = $"Not enough stock for '{p.Name}'. Available: {p.StockQty}.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Create order
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var order = new Order
            {
                UserId = userId,
                Total = cart.Sum(i => i.UnitPrice * i.Qty)
                // CreatedAt/PlacedAt: keep default in model if present
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(); // ensure Order.Id

            // Create order items + reduce stock
            foreach (var line in cart)
            {
                var p = products.First(x => x.Id == line.ProductId);

                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = p.Id,
                    Qty = line.Qty,
                    UnitPrice = line.UnitPrice
                });

                // Reduce stock
                p.StockQty -= line.Qty;
            }

            await _db.SaveChangesAsync();

            // Clear cart session and show success
            HttpContext.Session.Remove(CartKey);
            TempData["OrderId"] = order.Id;
            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success() => View();
    }
}
