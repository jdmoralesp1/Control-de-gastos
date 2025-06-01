using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Modelos.Wrapper;
using PruebaTecnicaMVC.Utilidades;

namespace PruebaTecnicaMVC.Areas.Movimientos.Controllers;

[Area("Movimientos")]
[Authorize(Roles = StaticDefinitions.Role_Admin)]
public class DepositoController : Controller
{
    private readonly IRepository<Deposito> depositoRepository;
    private readonly IRepository<FondoMonetario> fondoMonetarioRepository;
    private readonly IUnitOfWork unitOfWork;

    public DepositoController(IRepository<Deposito> depositoRepository, IRepository<FondoMonetario> fondoMonetarioRepository, IUnitOfWork unitOfWork)
    {
        this.depositoRepository = depositoRepository;
        this.fondoMonetarioRepository = fondoMonetarioRepository;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        Deposito? deposito = new();

        if (!id.HasValue)
        {
            deposito.Fecha = TimeUtil.GetDateTimeColombia();
            return View(deposito);
        }

        deposito = await depositoRepository.GetById(id.Value);

        if (deposito is null)
            return NotFound();

        return View(deposito);
    }

    #region API

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Deposito> fondosMonetarios = await depositoRepository
                                                        .GetAsync(
                                                            whereCondition: x => x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"), //TODO cambiar cuando ya acceda al id del usuario
                                                            orderBy: x => x.OrderByDescending(x => x.Fecha), 
                                                            includeProperties: nameof(FondoMonetario)
                                                        );

        IEnumerable<DepositoDto> fondosMonetarioResponse = fondosMonetarios.Select(x => new DepositoDto(
            x.Id,
            x.Fecha,
            x.FondoMonetario.Nombre,
            StringUtil.ConvertToMoneyFormat(x.Monto),
            x.Observaciones
        ));

        return Json(new { data = fondosMonetarioResponse });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(Deposito deposito)
    {
        if(deposito.FondoMonetarioId == 1)
        {
            TempData[StaticDefinitions.Error] = "Debe seleccionar un fondo monetario.";
            return View(deposito);
        }

        FondoMonetario? fondoMonetario = await fondoMonetarioRepository.GetById(deposito.FondoMonetarioId);

        if (fondoMonetario is null)
        {
            TempData[StaticDefinitions.Error] = "El fondo monetario seleccionado no existe, por favor verifique.";
            return View(deposito);
        }

        fondoMonetario.SaldoActual += deposito.Monto;
        await fondoMonetarioRepository.UpdateAsync(fondoMonetario);

        if (deposito.Id == 0)
        {
            deposito.UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"); //TODO cambiar cuando se implemente el identity
            await depositoRepository.AddAsync(deposito);
            TempData[StaticDefinitions.Exitosa] = "Se ha creado correctamente el deposito.";
        }
        else
        {
            await depositoRepository.UpdateAsync(deposito);
            TempData[StaticDefinitions.Exitosa] = "Se ha actualizado correctamente el deposito.";
        }

        await unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        Deposito? deposito = await depositoRepository.GetById(id);
        if (deposito is null)
            return Json(new Response<bool>($"No se encontro el deposito con id {id}"));

        FondoMonetario? fondoMonetario = await fondoMonetarioRepository.GetById(deposito.FondoMonetarioId);

        if (fondoMonetario is null)
        {
            TempData[StaticDefinitions.Error] = "El fondo monetario seleccionado no existe, por favor verifique.";
            return View(deposito);
        }

        fondoMonetario.SaldoActual -= deposito.Monto;
        await fondoMonetarioRepository.UpdateAsync(fondoMonetario);

        depositoRepository.Delete(deposito);

        await unitOfWork.Save();

        return Json(new Response<bool>(true, message: $"Se ha eliminado correctamente el deposito"));
    }

    #endregion
}
