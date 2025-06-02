using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.Wrapper;
using PruebaTecnicaMVC.Utilidades;

namespace PruebaTecnicaMVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class FondoMonetarioController : Controller
{
    private readonly IRepository<FondoMonetario> fondoMonetarioRepository;
    private readonly IUnitOfWork unitOfWork;

    public FondoMonetarioController(IRepository<FondoMonetario> fondoMonetarioRepository, IUnitOfWork unitOfWork)
    {
        this.fondoMonetarioRepository = fondoMonetarioRepository;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        FondoMonetario? fondoMonetario = new();

        if (!id.HasValue)
        {
            fondoMonetario.Activo = true;
            return View(fondoMonetario);
        }

        fondoMonetario = await fondoMonetarioRepository.GetById(id.Value);

        if (fondoMonetario is null)
            return NotFound();

        return View(fondoMonetario);
    }

    #region API

    [HttpGet]
    public async Task<IActionResult> GetAll(int inicial)
    {
        List<FondoMonetario> fondosMonetarios = (await fondoMonetarioRepository.GetAsync(x => x.Activo && x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"), 
                                                                                        x => x.OrderBy(x => x.Id == 1 ? 0 : 1)
                                                                                                .ThenBy(x => x.Nombre))).ToList(); //TODO cambiar cuando ya acceda al id del usuario

        if (inicial == 1)
        {
            if (fondosMonetarios.Count > 0)
                fondosMonetarios.RemoveAt(0);
        }

        IEnumerable<FondoMonetarioDto> fondosMonetarioResponse = fondosMonetarios.Select(x => new FondoMonetarioDto(
            x.Id,
            x.Nombre,
            x.Tipo,
            x.NumeroCuenta ?? "Sin numero de cuenta",
            x.SaldoActual,
            StringUtil.ConvertToMoneyFormat(x.SaldoActual),
            TimeUtil.DateTimeToFormatDDMMYYYYHHmm(x.FechaCreacion)
        ));

        return Json(new { data = fondosMonetarioResponse });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(FondoMonetario fondoMonetario)
    {
        if (!ModelState.IsValid)
        {
            TempData[StaticDefinitions.Error] = "Error al grabar el fondo monetario.";
            return View(fondoMonetario);
        }

        var tipoGastoExistente = await fondoMonetarioRepository
                                        .GetFirstOrDefaultAsync(x =>
                                            x.Nombre.ToLower().Trim() == fondoMonetario.Nombre.ToLower().Trim() &&
                                            x.Activo == true &&
                                            (fondoMonetario.Id == 0 || x.Id != fondoMonetario.Id) &&
                                            x.Id != 1
                                        );

        if (tipoGastoExistente is not null)
        {
            TempData[StaticDefinitions.Error] = "Ya existe un fondo monetario con el mismo nombre.";
            return View(fondoMonetario);
        }

        if (fondoMonetario.Id == 0)
        {
            fondoMonetario.UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"); //TODO cambiar cuando se implemente el identity
            await fondoMonetarioRepository.AddAsync(fondoMonetario);
            TempData[StaticDefinitions.Exitosa] = "Se ha creado correctamente el fondo monetario.";
        }
        else
        {
            await fondoMonetarioRepository.UpdateAsync(fondoMonetario);
            TempData[StaticDefinitions.Exitosa] = "Se ha actualizado correctamente el fondo monetario.";
        }

        await unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        FondoMonetario? fondoMonetario = await fondoMonetarioRepository.GetById(id);
        if (fondoMonetario is null)
            return Json(new Response<bool>($"No se encontro el fondo monetario con id {id}"));

        fondoMonetario.Activo = false;

        await fondoMonetarioRepository.UpdateAsync(fondoMonetario);

        await unitOfWork.Save();

        return Json(new Response<bool>(true, message: $"Se ha eliminado correctamente el fondo monetario"));
    }

    #endregion
}
