using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;
using PruebaTecnicaMVC.Modelos.DTOs;
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
            string[]? months = [];
            string[]? years = [];
            List<decimal>? data = [];

            IEnumerable<Presupuesto> presupuestos = await presupuestoRepository.GetAsync(
                                                            whereCondition: x => 
                                                                                x.Activo && 
                                                                                x.UsuarioId == userId &&
                                                                                (!anio.HasValue || x.Anio == anio),
                                                            orderBy: x => x.OrderBy(x => x.Anio).ThenBy(x => x.Mes));

            IEnumerable<GastoEncabezado>? gastos = (await gastoEncabezadoRepository.GetAsync(
                                                            whereCondition: x =>
                                                                (!anio.HasValue || x.Fecha >= new DateTime(anio.Value, 1, 1)) &&
                                                                (!anio.HasValue || x.Fecha <= new DateTime(anio.Value, 12, 31, 23, 59, 59)) &&
                                                                x.UsuarioId == userId,
                                                            orderBy: x => x.OrderBy(x => x.Fecha),
                                                            includeProperties: nameof(GastoEncabezado.Detalles)
                                                        )
                                            );

            if (!presupuestos.Any() && !gastos.Any())
                return Json(new { months, years, data, dataGastos });

            var gastosPorAnioMes = gastos
                                    .GroupBy(x => new AnioYMesDto { Anio = x.Fecha.Year, Mes = x.Fecha.Month })
                                    .OrderBy(g => g.Key.Anio)
                                    .ThenBy(g => g.Key.Mes)
                                    .ToList();

            HashSet<int> monthInNumbers = (presupuestos.Select(x => x.Mes).Union(gastosPorAnioMes.Select(x => x.Key.Mes))).ToHashSet();
            HashSet<int> yearsInNumbers = gastosPorAnioMes.Select(x => x.Key.Anio).Union(presupuestos.Select(x => x.Anio)).ToHashSet();

            months = monthInNumbers.Select(x => StringUtil.GetMonthName(x)).ToArray();
            years = yearsInNumbers.Select(x => x.ToString()).ToArray();

            if (!anio.HasValue)
                return Json(new { months, years, data, dataGastos });

            foreach (var itemYear in yearsInNumbers)
            {
                foreach (var itemMonth in monthInNumbers)
                {
                    if (presupuestos.Any())
                    {
                        var presupuesto = presupuestos.FirstOrDefault(x => x.Anio == itemYear && x.Mes == itemMonth);
                        
                        data.Add(presupuesto is not null ? presupuesto.MontoTotal : 0);
                    }
                    else
                    {
                        data.Add(0);
                    }

                    if (gastosPorAnioMes.Any())
                    {
                        var gasto = gastosPorAnioMes.FirstOrDefault(x => x.Key.Anio == itemYear && x.Key.Mes == itemMonth);

                        dataGastos.Add(gasto is not null ? gasto.Sum(x => x.MontoTotal) : 0);
                    }
                    else
                    {
                        dataGastos.Add(0);
                    }
                }
            }

            return Json(new { months, years, data, dataGastos });

            #endregion

        } 
    }
}
