using courierOptimisation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace courierOptimisation.Controllers
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
            var p = new PathsFinder();
            p.traverseSolutions();
            Console.WriteLine(p.bestPathsCost);
            foreach (var path in p.bestPaths)
            {
                Console.WriteLine(String.Join(" ", path));
            }
            return View(p.bestPaths);
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