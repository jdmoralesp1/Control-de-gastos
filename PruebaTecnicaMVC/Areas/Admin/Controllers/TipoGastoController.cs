using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.Repositories.TiposGasto;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.Wrapper;
using PruebaTecnicaMVC.Utilidades;

namespace PruebaTecnicaMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class TipoGastoController : Controller
{
    private readonly IRepository<TipoGasto> tipoGastoGenericRepository;
    private readonly ITipoGastoRepository tipoGastoRepository;
    private readonly IUnitOfWork unitOfWork;

    public TipoGastoController(IRepository<TipoGasto> tipoGastoGenericRepository, ITipoGastoRepository tipoGastoRepository, IUnitOfWork unitOfWork)
    {
        this.tipoGastoGenericRepository = tipoGastoGenericRepository;
        this.tipoGastoRepository = tipoGastoRepository;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        TipoGasto? tipoGasto = new();

        if (!id.HasValue)
        {
            return View(tipoGasto);
        }

        tipoGasto = await tipoGastoGenericRepository.GetById(id.Value);

        if (tipoGasto is null)
            return NotFound();

        return View(tipoGasto);
    }

    #region API

    [HttpGet]
    public async Task<IActionResult> GetAll(int inicial)
    {
        List<TipoGasto> tipoGastos = tipoGastoRepository.GetAllAsync("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"); //TODO cambiar cuando ya acceda al id del usuario
        if (inicial == 1)
        {
            if (tipoGastos.Count > 0)
                tipoGastos.RemoveAt(0);
        }

        IEnumerable<TipoGastoDto> tipoGastosResponse = tipoGastos.Select(x => new TipoGastoDto(
            x.Id,
            x.Codigo,
            x.Nombre,
            x.Descripcion ?? "Sin descripción",
            TimeUtil.DateTimeToFormatDDMMYYYYHHmm(x.FechaCreacion)
        ));

        return Json(new { data = tipoGastosResponse });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(TipoGasto tipoGasto)
    {
        if (!ModelState.IsValid)
        {
            TempData[StaticDefinitions.Error] = "Error al grabar el tipo de gasto.";
            return View(tipoGasto);
        }

        var tipoGastoExistente = await tipoGastoGenericRepository
                                        .GetFirstOrDefaultAsync(x => 
                                            x.Nombre.ToLower().Trim() == tipoGasto.Nombre.ToLower().Trim() &&
                                            x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47") && //TODO cambiar cuando ya acceda al id del usuario
                                            x.Activo == true &&
                                            (tipoGasto.Id == 0 || x.Id != tipoGasto.Id) &&
                                            x.Id != 1
                                        );

        if(tipoGastoExistente is not null)
        {
            TempData[StaticDefinitions.Error] = "Ya existe un tipo de gasto con el mismo nombre.";
            return View(tipoGasto);
        }

        if (tipoGasto.Id == 0)
        {
            var ultimoTipoGasto = await tipoGastoGenericRepository.GetLastBy(x => x.Id);

            tipoGasto.Codigo = ultimoTipoGasto is null ? $"TG1" : $"TG{(ultimoTipoGasto.Id + 1)}";

            tipoGasto.UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"); //TODO cambiar cuando se implemente el identity

            await tipoGastoGenericRepository.AddAsync(tipoGasto);
            TempData[StaticDefinitions.Exitosa] = "Se ha creado correctamente el tipo de gasto.";
        }
        else
        {
            await tipoGastoGenericRepository.UpdateAsync(tipoGasto);
            TempData[StaticDefinitions.Exitosa] = "Se ha actualizado correctamente el tipo de gasto.";
        }

        await unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        TipoGasto? tipoGasto = await tipoGastoGenericRepository.GetById(id);
        if (tipoGasto is null)
            return Json(new Response<bool>($"No se encontro el tipo de gasto con id {id}"));

        tipoGasto.Activo = false;

        await tipoGastoGenericRepository.UpdateAsync(tipoGasto);

        await unitOfWork.Save();

        return Json(new Response<bool>(true, message: $"Se ha eliminado correctamente el tipo gasto"));
    }

    #endregion

}
