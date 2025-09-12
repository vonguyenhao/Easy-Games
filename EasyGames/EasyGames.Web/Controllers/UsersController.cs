using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EasyGames.Web.Controllers
{
    // Owner-only user management screen
    [Authorize(Roles = "Owner")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userMgr;

        public UsersController(UserManager<IdentityUser> userMgr)
        {
            _userMgr = userMgr;
        }

        // Simple view model for creating users
        public class CreateUserVm
        {
            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
            public string Password { get; set; } = "";
        }

        // GET: /Users
        public IActionResult Index()
        {
            // NOTE: For large user sets you'd paginate; here we keep it simple
            var users = _userMgr.Users.ToList();
            return View(users);
        }

        // GET: /Users/Create
        public IActionResult Create() => View(new CreateUserVm());

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Create a user; by default we add them to the Customer role
            var u = new IdentityUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true
            };

            var result = await _userMgr.CreateAsync(u, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _userMgr.AddToRoleAsync(u, "Customer"); // default role
            TempData["Msg"] = $"User '{vm.Email}' created as Customer.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var u = await _userMgr.FindByIdAsync(id);
            if (u == null) return NotFound();

            // Prevent deleting yourself or the seeded admin
            if (User.Identity?.Name?.Equals(u.Email, System.StringComparison.OrdinalIgnoreCase) == true
                || u.Email?.Equals("admin@easygames.com", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                TempData["Err"] = "You cannot delete this account.";
                return RedirectToAction(nameof(Index));
            }

            var res = await _userMgr.DeleteAsync(u);
            TempData[res.Succeeded ? "Msg" : "Err"] = res.Succeeded ? "User deleted." :
                string.Join("; ", res.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }
    }
}
