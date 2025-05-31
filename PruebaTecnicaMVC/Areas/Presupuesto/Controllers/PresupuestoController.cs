using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.Wrapper;
using PruebaTecnicaMVC.Utilidades;
using System.Globalization;
using System.Linq;

namespace PruebaTecnicaMVC.Areas.Presupuestos.Controllers;

[Area("Presupuesto")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class PresupuestoController : Controller
{
    private readonly IRepository<Presupuesto> presupuestoRepository;
    private readonly IUnitOfWork unitOfWork;

    public PresupuestoController(IRepository<Presupuesto> presupuestoRepository, IUnitOfWork unitOfWork)
    {
        this.presupuestoRepository = presupuestoRepository;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        Presupuesto? presupuesto = new();

        if (!id.HasValue)
        {
            return View(presupuesto);
        }

        presupuesto = await presupuestoRepository.GetById(id.Value);

        if (presupuesto is null)
            return NotFound();

        return View(presupuesto);
    }


    #region API

    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Presupuesto> presupuestos = await presupuestoRepository
                                                        .GetAsync(
                                                            whereCondition: x => x.Activo == true && x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"), //TODO cambiar cuando ya acceda al id del usuario
                                                            orderBy: x => x.OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes));

        IEnumerable<PresupuestoDto> presupuestosResponse = presupuestos.Select(x => new PresupuestoDto(
            x.Id,
            StringUtil.GetMonthName(x.Mes),
            x.Anio,
            StringUtil.ConvertToMoneyFormat(x.MontoTotal),
            TimeUtil.DateTimeToFormatDDMMYYYYHHmm(x.FechaCreacion)
        ));

        return Json(new { data = presupuestosResponse });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(Presupuesto presupuesto)
    {
        if (!ModelState.IsValid)
        {
            TempData[StaticDefinitions.Error] = "Error al grabar el presupuesto.";
            return View(presupuesto);
        }

        var tipoGastoExistente = await presupuestoRepository
                                        .GetFirstOrDefaultAsync(x =>
                                            x.Anio == presupuesto.Anio &&
                                            x.Mes == presupuesto.Mes &&
                                            x.Activo == true &&
                                            x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47") && //TODO cambiar cuando ya acceda al id del usuario
                                            (presupuesto.Id == 0 || x.Id != presupuesto.Id)
                                        );

        if (tipoGastoExistente is not null)
        {
            TempData[StaticDefinitions.Error] = "Ya existe un presupuesto con el mes y año ingresado.";
            return View(presupuesto);
        }

        if (presupuesto.Id == 0)
        {
            presupuesto.UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"); //TODO cambiar cuando se implemente el identity
            await presupuestoRepository.AddAsync(presupuesto);
            TempData[StaticDefinitions.Exitosa] = "Se ha creado correctamente el presupuesto.";
        }
        else
        {
            await presupuestoRepository.UpdateAsync(presupuesto);
            TempData[StaticDefinitions.Exitosa] = "Se ha actualizado correctamente el presupuesto.";
        }

        await unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        Presupuesto? presupuesto = await presupuestoRepository.GetById(id);
        if (presupuesto is null)
            return Json(new Response<bool>($"No se encontro el presupuesto con id {id}"));

        presupuesto.Activo = false;

        await presupuestoRepository.UpdateAsync(presupuesto);

        await unitOfWork.Save();

        return Json(new Response<bool>(true, message: $"Se ha eliminado correctamente el presupuesto"));
    }

    [HttpPost]
    public void SetTempDataPresupuestoId(int id)
    {
        TempData["PresupuestoId"] = id;
    }
    #endregion
}
