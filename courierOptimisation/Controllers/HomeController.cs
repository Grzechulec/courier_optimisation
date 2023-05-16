using courierOptimisation.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Web;

namespace courierOptimisation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IndexModel _model;

        public HomeController(ILogger<HomeController> logger,
            IndexModel model)
        {
            _logger = logger;
            _model = model;
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
            return View(_model);
        }

        public IActionResult Test()
        {
            Console.WriteLine("aaa");
            List<(int, int)> points = new() { (0, 0), (0, 3), (4, 0), (99, 99) };
            _model.Paths = DistanceMatrixHelper.GenerateDistanceMatrix(_model.Points);
            return View("Index", _model);
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            List<string> fileContent = new();

            if (file != null)
            {
                string? line;
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    while((line = reader.ReadLine()) != null)
                    {
                        fileContent.Add(line);
                    }
                }
            }
            _model.Points = DistanceMatrixHelper.ConvertStringsToPoints(fileContent);

            return View("Index", _model);
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