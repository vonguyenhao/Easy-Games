using System.Diagnostics;
using EasyGames.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyGames.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Friendly status page for 404/403/etc.
        // This is invoked by UseStatusCodePagesWithReExecute in Program.cs
        public IActionResult StatusCode(int code)
        {
            ViewBag.Code = code; // pass the status code to the view
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
