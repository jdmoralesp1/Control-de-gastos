using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.Modelos.ViewModel;
using PruebaTecnicaMVC.Utilidades;
using System.Diagnostics;

namespace PruebaTecnicaMVC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = StaticDefinitions.Role_Admin)]
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ErrorGenerico(int code)
        {
            if (code == 404)
                return View("Error404");
            return View("Error");
        }

    }
}
