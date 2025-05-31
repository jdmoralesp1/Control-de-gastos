using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.Wrapper;
using PruebaTecnicaMVC.Utilidades;
using System;
using System.Collections;

namespace PruebaTecnicaMVC.Areas.Presupuestos.Controllers;

[Area("Presupuesto")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class PresupuestoDetalleController : Controller
{
    private readonly IRepository<PresupuestoDetalle> presupuestoDetalleRepository;
    private readonly IRepository<Presupuesto> presupuestoRepository;
    private readonly IRepository<TipoGasto> tipoGastoRepository;
    private readonly IUnitOfWork unitOfWork;

    public PresupuestoDetalleController(IRepository<PresupuestoDetalle> presupuestoDetalleRepository, IRepository<Presupuesto> presupuestoRepository, IRepository<TipoGasto> tipoGastoRepository, IUnitOfWork unitOfWork)
    {
        this.presupuestoDetalleRepository = presupuestoDetalleRepository;
        this.presupuestoRepository = presupuestoRepository;
        this.tipoGastoRepository = tipoGastoRepository;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        string? fechapresupuesto = TempData["FechaPresupuesto"] as string;
        if(!fechapresupuesto.IsNullOrEmpty())
            TempData.Keep("FechaPresupuesto");

        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        PresupuestoDetalle? presupuestoDetalle = new();

        if (!id.HasValue)
        {
            return View(presupuestoDetalle);
        }

        presupuestoDetalle = await presupuestoDetalleRepository.GetById(id.Value);

        if (presupuestoDetalle is null)
            return NotFound();

        return View(presupuestoDetalle);
    }

    #region API

    public async Task<IActionResult> GetAll()
    {
        int? presupuestoId = TempData["PresupuestoId"] as int?;
        if (presupuestoId is null)
        {
            TempData[StaticDefinitions.Error] = "Error al obtener el detalle del presupuesto, intentelo de nuevo.";
            return View(nameof(Index));
        }

        TempData.Keep("PresupuestoId");

        IEnumerable<PresupuestoDetalle> presupuestosDetalle = 
            await presupuestoDetalleRepository.GetAllAsyncInclude(predicate: x => x.PresupuestoId == presupuestoId.Value && x.Presupuesto.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"), //TODO cambiar cuando ya acceda al id del usuario
                                                                    include: x => x.Include(x => x.Presupuesto).Include(x => x.TipoGasto));

        IEnumerable<PresupuestoDetalleDto> presupuestosResponse = presupuestosDetalle.Select(x => new PresupuestoDetalleDto(
            x.Id,
            x.TipoGasto.Nombre,
            StringUtil.ConvertToMoneyFormat(x.MontoPresupuestado)
            
        ));

        string titulo = string.Empty;

        Presupuesto? presupuesto = presupuestosDetalle.FirstOrDefault()?.Presupuesto;

        if (presupuesto is not null)
            titulo = $" - {StringUtil.GetMonthName(presupuesto.Mes)} {presupuesto.Anio}";


        return Json(new { data = presupuestosResponse, titulo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(PresupuestoDetalle presupuestoDetalle)
    {
        int? presupuestoId = TempData["PresupuestoId"] as int?;
        if(presupuestoId is null)
        {
            TempData[StaticDefinitions.Error] = "Error al grabar el detalle del presupuesto, intentelo de nuevo.";
            return View(presupuestoDetalle);
        }

        TempData.Keep("PresupuestoId");

        if(presupuestoDetalle.TipoGastoId == 1)
        {
            TempData[StaticDefinitions.Error] = "Debe seleccionar un tipo de gasto de la lista";
            return View(presupuestoDetalle);
        }

        TipoGasto? tipoGasto = await tipoGastoRepository.GetById(presupuestoDetalle.TipoGastoId);
        if(tipoGasto is null)
        {
            TempData[StaticDefinitions.Error] = "El tipo de gasto seleccionado no existe, por favor verifique.";
            return View(presupuestoDetalle);
        }


        Presupuesto? presupuestoAModificar = await presupuestoRepository.GetById(presupuestoId.Value);
        if(presupuestoAModificar is null)
        {
            TempData[StaticDefinitions.Error] = "El presupuesto al que se desea agregar el detalle no existe, por favor verifique.";
            return View(presupuestoDetalle);
        }

        PresupuestoDetalle presupuestoDetalleOriginal = new();
        if (presupuestoDetalle.Id != 0)
            presupuestoDetalleOriginal = (await presupuestoDetalleRepository.GetById(presupuestoDetalle.Id))!;

        if (await presupuestoDetalleRepository.GetAsyncInclude(
                                                predicate: x =>
                                                    x.TipoGastoId == presupuestoDetalle.TipoGastoId &&
                                                    x.Presupuesto.Anio == presupuestoAModificar.Anio &&
                                                    x.Presupuesto.Mes == presupuestoAModificar.Mes &&
                                                    (presupuestoDetalle.Id == 0 || x.Id != presupuestoId),
                                                include: x => x.Include(x => x.Presupuesto)
                                        ) is not null)
        {
            TempData[StaticDefinitions.Error] = "Ya existe un detalle del presupuesto con el tipo de gasto ingresado.";
            return View(presupuestoDetalle);
        }


        IEnumerable<PresupuestoDetalle>? presupuestosDetalles = await presupuestoDetalleRepository.GetAsync(x => x.PresupuestoId == presupuestoId);

        presupuestoAModificar.MontoTotal = presupuestosDetalles.Any() 
                                            ? presupuestosDetalles.Where(x => x.Id != presupuestoDetalle.Id).Sum(x => x.MontoPresupuestado) + presupuestoDetalle.MontoPresupuestado 
                                            : presupuestoDetalle.MontoPresupuestado;

        await presupuestoRepository.UpdateAsync(presupuestoAModificar);

        if (presupuestoDetalle.Id == 0)
        {
            presupuestoDetalle.PresupuestoId = presupuestoId.Value;
            await presupuestoDetalleRepository.AddAsync(presupuestoDetalle);
            TempData[StaticDefinitions.Exitosa] = "Se ha creado correctamente el detalle del presupuesto.";
        }
        else
        {
            presupuestoDetalleOriginal.MontoPresupuestado = presupuestoDetalle.MontoPresupuestado;
            presupuestoDetalleOriginal.TipoGastoId = presupuestoDetalle.TipoGastoId;

            await presupuestoDetalleRepository.UpdateAsync(presupuestoDetalleOriginal);
            TempData[StaticDefinitions.Exitosa] = "Se ha actualizado correctamente el detalle del presupuesto.";
        }

        await unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        PresupuestoDetalle? presupuestoDetalle = await presupuestoDetalleRepository.GetById(id);
        if (presupuestoDetalle is null)
            return Json(new Response<bool>($"No se encontro el detalle del presupuesto con id {id}"));

        Presupuesto? presupuestoAModificar = await presupuestoRepository.GetById(presupuestoDetalle.PresupuestoId);
        if (presupuestoAModificar is null)
        {
            return Json(new Response<bool>($"El presupuesto del detalle no existe, por favor verifique"));
        }
        presupuestoAModificar.MontoTotal -= presupuestoDetalle.MontoPresupuestado;

        presupuestoDetalleRepository.Delete(presupuestoDetalle);
        await presupuestoRepository.UpdateAsync(presupuestoAModificar);

        await unitOfWork.Save();

        return Json(new Response<bool>(true, message: $"Se ha eliminado correctamente el detalle del presupuesto"));
    }

    #endregion
}
