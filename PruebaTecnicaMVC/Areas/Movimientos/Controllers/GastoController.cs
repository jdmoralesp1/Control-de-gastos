using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.Wrapper;
using PruebaTecnicaMVC.Utilidades;

namespace PruebaTecnicaMVC.Areas.Movimientos.Controllers;

[Area("Movimientos")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class GastoController : Controller
{
    private readonly IRepository<GastoEncabezado> gastoEncabezadoRepository;
    private readonly IRepository<FondoMonetario> fondoMonetarioRepository;
    private readonly IRepository<TipoGasto> tipoGastoRepository;
    private readonly IRepository<Presupuesto> presupuestoRepository;
    private readonly IGastoService gastoService;
    private readonly IUnitOfWork unitOfWork;
    private bool? esPrimeraValidacion = null;
    private int? fondoMonetarioId = null;

    public GastoController(IRepository<GastoEncabezado> gastoEncabezadoRepository, IRepository<FondoMonetario> fondoMonetarioRepository, IRepository<TipoGasto> tipoGastoRepository, IRepository<Presupuesto> presupuestoRepository, IGastoService gastoService, IUnitOfWork unitOfWork)
    {
        this.gastoEncabezadoRepository = gastoEncabezadoRepository;
        this.fondoMonetarioRepository = fondoMonetarioRepository;
        this.tipoGastoRepository = tipoGastoRepository;
        this.presupuestoRepository = presupuestoRepository;
        this.gastoService = gastoService;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        GastoDto? gastoDto = new GastoDto
        {
            GastoEncabezado = new GastoEncabezado(),
            Detalle = new List<GastoDetalle> { new GastoDetalle() } // Al menos uno por defecto
        };

        if (!id.HasValue)
        {
            gastoDto.GastoEncabezado.Fecha = TimeUtil.GetDateTimeColombia();
            return View(gastoDto);
        }

        GastoEncabezado gastoEncabezado = (await gastoEncabezadoRepository.GetAsyncInclude(x => x.Id == id.Value, x => x.Include(x => x.FondoMonetario).Include(x => x.Detalles)))!;

        gastoDto.GastoEncabezado = gastoEncabezado;
        gastoDto.Detalle = (List<GastoDetalle>)gastoEncabezado.Detalles;

        if (gastoDto is null)
            return NotFound();

        return View(gastoDto);
    }

    #region API

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<GastoEncabezado> gastosEncabezado = await gastoEncabezadoRepository
                                                                .GetAsync(
                                                                    whereCondition: x => x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"), //TODO cambiar cuando ya acceda al id del usuario
                                                                    orderBy: x => x.OrderBy(x => x.Fecha),
                                                                    includeProperties: nameof(FondoMonetario)
                                                                );

        IEnumerable<GastoEncabezadoDto> gastosEncabezadoResponse = gastosEncabezado.Select(x => new GastoEncabezadoDto(
            x.Id,
            TimeUtil.DateTimeToFormatDDMMYYYY(x.Fecha),
            x.FondoMonetario.Nombre,
            x.NombreComercio,
            x.TipoDocumento,
            x.NumeroDocumento,
            x.MontoTotal,
            StringUtil.ConvertToMoneyFormat(x.MontoTotal),
            x.Observaciones
        ));

        return Json(new { data = gastosEncabezadoResponse });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(GastoDto gasto)
    {
        List<(string, string)> listaRespuestaUpsert = await gastoService.Upsert(gasto);

        if (listaRespuestaUpsert.FirstOrDefault().Item1 == StaticDefinitions.Error)
        {
            TempData[StaticDefinitions.Error] = listaRespuestaUpsert.FirstOrDefault().Item2;
            return View(gasto);
        }

        foreach (var respuesta in listaRespuestaUpsert)
        {
            if (respuesta.Item1 == StaticDefinitions.Advertencia)
            {
                TempData[StaticDefinitions.Advertencia] = respuesta.Item2;
            }
            else             {
                TempData[StaticDefinitions.Exitosa] = respuesta.Item2;
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        GastoEncabezado? gastoEncabezado = await gastoEncabezadoRepository.GetById(id);
        if (gastoEncabezado is null)
            return Json(new Response<bool>($"No se encontro el gasto con id {id}"));

        gastoEncabezadoRepository.Delete(gastoEncabezado);

        await unitOfWork.Save();

        return Json(new Response<bool>(true, message: $"Se ha eliminado correctamente el gasto"));
    }

    #endregion
}
