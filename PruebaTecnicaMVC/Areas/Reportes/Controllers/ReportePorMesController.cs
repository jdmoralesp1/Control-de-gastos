using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Utilidades;

namespace PruebaTecnicaMVC.Areas.Reportes.Controllers;

[Area("Reportes")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class ReportePorMesController : Controller
{
    private readonly IRepository<Presupuesto> presupuestoRepository;
    private readonly IRepository<GastoEncabezado> gastoEncabezadoRepository;
    private readonly IRepository<TipoGasto> tipoGastoRepository;
    private readonly IIdentityService identityService;

    public ReportePorMesController(IRepository<Presupuesto> presupuestoRepository, IRepository<GastoEncabezado> gastoEncabezadoRepository, IRepository<TipoGasto> tipoGastoRepository, IIdentityService identityService)
    {
        this.presupuestoRepository = presupuestoRepository;
        this.gastoEncabezadoRepository = gastoEncabezadoRepository;
        this.tipoGastoRepository = tipoGastoRepository;
        this.identityService = identityService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerReportePorAñoYMes(int anio, int mes)
    {
        Guid userId = identityService.ObtenerUsuarioId();
        List<decimal> dataGastos = [];

        IEnumerable<Presupuesto> todosLosPresupuestos = presupuestoRepository.GetAsyncAsNoTracking(x => x.Activo && x.UsuarioId == userId);
        IEnumerable<GastoEncabezado> todosLosGastos = gastoEncabezadoRepository.GetAsyncAsNoTracking(x => x.UsuarioId == userId);
        string[]? years = todosLosPresupuestos.Select(x => x.Anio).Union(todosLosGastos.Select(x => x.Fecha.Year)).OrderBy(x => x).Select(x => x.ToString()).ToHashSet().ToArray();

        IEnumerable<Presupuesto> presupuestos = await presupuestoRepository.GetAsync(
                                                        whereCondition: x =>
                                                                            x.Activo &&
                                                                            x.UsuarioId == userId &&
                                                                            x.Anio == anio &&
                                                                            x.Mes == mes,
                                                        orderBy: x => x.OrderBy(x => x.Anio).ThenBy(x => x.Mes),
                                                        includeProperties: nameof(Presupuesto.Detalles));

        List<string> tiposGasto = [];
        List<decimal> presupuestosList = [];
        List<decimal> gastosList = [];


        Dictionary<int, string> tiposGastoDictionary = (await tipoGastoRepository.GetAsync()).ToDictionary(x => x.Id, x => x.Nombre);
        var presupuestoPorTipoGasto = presupuestos.SelectMany(x => x.Detalles).GroupBy(x => x.TipoGastoId).ToDictionary(x => x.Key, x => x.ToList());


        int totalDaysInmonth = DateTime.DaysInMonth(anio, mes);

        IEnumerable<GastoEncabezado> gastos = await gastoEncabezadoRepository.GetAsync(
                                                            whereCondition: x =>
                                                                x.Fecha >= new DateTime(anio, mes, 1) &&
                                                                x.Fecha <= new DateTime(anio, mes, totalDaysInmonth, 23, 59, 59) &&
                                                                x.UsuarioId == userId,
                                                            orderBy: x => x.OrderBy(x => x.Fecha),
                                                            includeProperties: nameof(GastoEncabezado.Detalles)
                                                        );

        if(!presupuestos.Any() && !gastos.Any())
            return Json(new { tiposGasto, presupuestosList, gastosList, years });

        Dictionary<int, List<GastoDetalle>> gastosPorTipoGasto = gastos
                                                                    .SelectMany(x => x.Detalles)
                                                                    .GroupBy(x => x.TipoGastoId)
                                                                    .ToDictionary(x => x.Key, x => x.ToList());

        HashSet<int> tposGastoExistentes = presupuestoPorTipoGasto.Select(x => x.Key).ToList().Union(gastosPorTipoGasto.Select(x => x.Key).ToList()).ToHashSet();

        foreach (var item in tposGastoExistentes)
        {
            if (tiposGastoDictionary.TryGetValue(item, out string? tipoGastoNombre))
                tiposGasto.Add(tipoGastoNombre);

            if (presupuestoPorTipoGasto.TryGetValue(item, out List<PresupuestoDetalle>? listaDetallesPresupuesto))
                presupuestosList.Add(listaDetallesPresupuesto.Sum(x => x.MontoPresupuestado));
            else
                presupuestosList.Add(0);

            if (gastosPorTipoGasto.TryGetValue(item, out List<GastoDetalle>? listaDetallesGasto))
                gastosList.Add(listaDetallesGasto.Sum(x => x.Monto));
            else
                gastosList.Add(0);
        }


        return Json(new { tiposGasto, presupuestosList, gastosList, years });
    }
}
