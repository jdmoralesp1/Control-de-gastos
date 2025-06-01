using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.ViewModel;
using PruebaTecnicaMVC.Utilidades;
using System.Diagnostics;

namespace PruebaTecnicaMVC.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = StaticDefinitions.Role_Admin)]
    public class HomeController : Controller
    {
        private readonly IRepository<Presupuesto> presupuestoRepository;
        private readonly IRepository<GastoEncabezado> gastoEncabezadoRepository;
        private readonly IIdentityService identityService;

        public HomeController(IRepository<Presupuesto> presupuestoRepository, IRepository<GastoEncabezado> gastoEncabezadoRepository, IIdentityService identityService)
        {
            this.presupuestoRepository = presupuestoRepository;
            this.gastoEncabezadoRepository = gastoEncabezadoRepository;
            this.identityService = identityService;
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
        public async Task<IActionResult> ObtenerPresupuestoVsGastos(int? anio)
        {
            Guid userId = identityService.ObtenerUsuarioId();
            List<decimal> dataGastos = [];

            IEnumerable<Presupuesto> presupuestos = await presupuestoRepository.GetAsync(
                                                            whereCondition: x => 
                                                                                x.Activo && 
                                                                                x.UsuarioId == userId &&
                                                                                (!anio.HasValue || x.Anio == anio),
                                                            orderBy: x => x.OrderBy(x => x.Anio).ThenBy(x => x.Mes));

            int[]monthInNumbers = presupuestos.Select(x => x.Mes).ToArray();

            string[]? months = monthInNumbers.Select(x => StringUtil.GetMonthName(x)).ToArray();
            string[]? years = presupuestos.Select(x => x.Anio.ToString()).ToHashSet().ToArray();
            decimal[]? data = presupuestos.Select(x => x.MontoTotal).ToArray();

            Presupuesto? presupuestInicial = presupuestos.FirstOrDefault();
            Presupuesto? presupuestFinal = presupuestos.LastOrDefault();

            if (presupuestos.Any())
            {
                int totalDaysInmonth = DateTime.DaysInMonth(presupuestFinal!.Anio, presupuestFinal.Mes);

                var gastos = (await gastoEncabezadoRepository.GetAsync(
                                                                whereCondition: x =>
                                                                    x.Fecha >= new DateTime(presupuestInicial!.Anio, presupuestInicial.Mes, 1) &&
                                                                    x.Fecha <= new DateTime(presupuestFinal!.Anio, presupuestFinal.Mes, totalDaysInmonth, 23, 59, 59) &&
                                                                    x.UsuarioId == userId,
                                                                orderBy: x => x.OrderBy(x => x.Fecha),
                                                                includeProperties: nameof(GastoEncabezado.Detalles)
                                                            )
                                                )
                                                .GroupBy(x => x.Fecha.Month)
                                                .ToDictionary(x=> x.Key, x => x.ToList());

                foreach (var item in monthInNumbers)
                {
                    if(gastos.TryGetValue(item, out List<GastoEncabezado>? gasto)){
                        dataGastos.Add(gasto.Sum(x => x.MontoTotal));
                        continue;
                    }

                    dataGastos.Add(0);
                }
            }

            return Json(new { months, years, data, dataGastos });

            #endregion

        } 
    }
}
