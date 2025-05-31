using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.ViewModel;
using PruebaTecnicaMVC.Utilidades;
using System.Diagnostics;
using PruebaTecnicaMVC.Utilidades;

namespace PruebaTecnicaMVC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = StaticDefinitions.Role_Admin)]
    public class HomeController : Controller
    {
        private readonly IRepository<Presupuesto> presupuestoRepository;

        public HomeController(IRepository<Presupuesto> presupuestoRepository)
        {
            this.presupuestoRepository = presupuestoRepository;
        }

        public IActionResult Index()
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

        #region API

        [HttpGet]
        public async Task<IActionResult> ObtenerPresupuestos(int? anio)
        {
            IEnumerable<Presupuesto> presupuestos = await presupuestoRepository.GetAsync(
                                                            whereCondition: x => 
                                                                                x.Activo && 
                                                                                x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47") &&
                                                                                (!anio.HasValue || x.Anio == anio),
                                                            orderBy: x => x.OrderBy(x => x.Anio).ThenBy(x => x.Mes));

            string[] months = presupuestos.Select(x => StringUtil.GetMonthName(x.Mes)).ToArray();
            string[] years = presupuestos.Select(x => x.Anio.ToString()).ToHashSet().ToArray();
            decimal[] data = presupuestos.Select(x => x.MontoTotal).ToArray();

            return Json(new { months, years, data });

            #endregion

        } 
    }
}
