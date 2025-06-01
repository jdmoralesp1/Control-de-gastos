using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;
using PruebaTecnicaMVC.Modelos.DTOs;
using PruebaTecnicaMVC.Modelos.Entities;
using PruebaTecnicaMVC.Utilidades;
using System.Text;

namespace PruebaTecnicaMVC.Aplicacion.Services;
public class GastoService : IGastoService
{
    private readonly IRepository<GastoEncabezado> gastoEncabezadoRepository;
    private readonly IRepository<FondoMonetario> fondoMonetarioRepository;
    private readonly IRepository<TipoGasto> tipoGastoRepository;
    private readonly IRepository<Presupuesto> presupuestoRepository;
    private readonly IUnitOfWork unitOfWork;
    private int month = 0;
    private int year = 0;

    public GastoService(IRepository<GastoEncabezado> gastoEncabezadoRepository, IRepository<FondoMonetario> fondoMonetarioRepository, IRepository<TipoGasto> tipoGastoRepository, IRepository<Presupuesto> presupuestoRepository, IUnitOfWork unitOfWork)
    {
        this.gastoEncabezadoRepository = gastoEncabezadoRepository;
        this.fondoMonetarioRepository = fondoMonetarioRepository;
        this.tipoGastoRepository = tipoGastoRepository;
        this.presupuestoRepository = presupuestoRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<List<(string, string)>> Upsert(GastoDto gasto)
    {
        List<(string, string)> respuesta = [];


        month = gasto.GastoEncabezado.Fecha.Month;
        year = gasto.GastoEncabezado.Fecha.Year;

        (string? errorValidacionesIniciales, Presupuesto? presupuesto, FondoMonetario? fondoMonetario) = await ValidacionesIniciales(gasto);
        if(errorValidacionesIniciales is not null)
        {
            respuesta.Add((StaticDefinitions.Error, errorValidacionesIniciales));
            return respuesta;
        }

        GastoEncabezado gastoEncabezado = gasto.GastoEncabezado;
        gastoEncabezado.MontoTotal = gasto.Detalle.Sum(x => x.Monto);
        gastoEncabezado.Detalles = gasto.Detalle;

        StringBuilder stringBuilder = await CalculosAlPresupuesto(gasto, presupuesto!, gastoEncabezado);

        (decimal nuevoSaldoFondoMonetario, string mensajeInsertUpdate) = gasto.GastoEncabezado.Id != 0 
                                                                            ? await ActualizarGastoEncabezado(gasto, gastoEncabezado, fondoMonetario!.SaldoActual)
                                                                            : await CrearGastoEncabezado(gasto, gastoEncabezado, fondoMonetario!.SaldoActual);
        respuesta.Add((StaticDefinitions.Exitosa, mensajeInsertUpdate));

        if (nuevoSaldoFondoMonetario < 0)
            stringBuilder.AppendLine($"<b>{fondoMonetario.Nombre}:</b> Ha sobregirado el fondo monetario quedando con un saldo de {StringUtil.ConvertToMoneyFormat(nuevoSaldoFondoMonetario)}");

        fondoMonetario.SaldoActual = nuevoSaldoFondoMonetario;
        await fondoMonetarioRepository.UpdateAsync(fondoMonetario);

        string advertencias = stringBuilder.ToString();
        if (!advertencias.IsNullOrEmpty())
            respuesta.Add((StaticDefinitions.Advertencia, advertencias));

        await unitOfWork.Save();

        return respuesta;
    }

    private async Task<(string?, Presupuesto?, FondoMonetario?)> ValidacionesIniciales(GastoDto gasto)
    {
        if (gasto.GastoEncabezado.FondoMonetarioId == StaticDefinitions.FondoMonetarioInitial)
            return ("Debe seleccionar un fondo monetario", null, null);

        if(gasto.Detalle.Exists(x => x.TipoGastoId == StaticDefinitions.TipoGastoInitial))
        {
            return ("Debe seleccionar un tipo de gasto en todos los detalles", null, null);
        }

        Presupuesto? presupuesto = await presupuestoRepository.GetAsyncInclude(
                                                    predicate: x =>
                                                        x.Activo == true &&
                                                        x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47") &&//TODO cambiar cuando se implemente el identity
                                                        x.Mes == month &&
                                                        x.Anio == year,
                                                    include: x => x.Include(x => x.Detalles)
                                                    );

        if (presupuesto is null)
        {
            return ("No existe un presupuesto para la fecha seleccionada, creelo e intentelo de nuevo.", null, null);
        }

        FondoMonetario? fondoMonetario = await fondoMonetarioRepository.GetById(gasto.GastoEncabezado.FondoMonetarioId);

        if (fondoMonetario is null)
        {
            return ("El fondo monetario seleccionado no existe, por favor verifique.", null, null);
        }

        if (gasto.Detalle.Select(x => x.TipoGastoId).Distinct().Count() != 1)
        {
            return ("Debe seleccionar un solo tipo de gasto los detalles del gasto.", null, null);
        }

        if(presupuesto.Detalles.FirstOrDefault(x => x.TipoGastoId == gasto.Detalle.FirstOrDefault()!.TipoGastoId) is null)
        {
            return ("El tipo de gasto seleccionado no existe en el presupuesto, por favor creelo antes de usarlo.", null, null);
        }

        return (null, presupuesto, fondoMonetario);
    }

    private async Task<StringBuilder> CalculosAlPresupuesto(GastoDto gasto, Presupuesto presupuesto, GastoEncabezado gastoEncabezado)
    {
        TipoGasto tipoGasto = (await tipoGastoRepository.GetById(gasto.Detalle.FirstOrDefault()!.TipoGastoId))!;

        int totalDaysInMonth = DateTime.DaysInMonth(year, month);

        IEnumerable<GastoEncabezado> gastosTotalesDelMes =
            await gastoEncabezadoRepository.GetAllAsyncInclude(
                                                        predicate: x => x.Fecha >= gasto.GastoEncabezado.Fecha.Date &&
                                                                        x.Fecha <= new DateTime(year, month, totalDaysInMonth, 23, 59, 59) &&
                                                                        x.UsuarioId == Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47") &&
                                                                        x.Detalles.Any(x => x.TipoGastoId == tipoGasto.Id),
                                                        include: x => x.Include(x => x.Detalles)
                                                    );

        decimal presupuestoDelMes = presupuesto.Detalles.FirstOrDefault(x => x.TipoGastoId == tipoGasto.Id)!.MontoPresupuestado;

        StringBuilder stringBuilder = new();

        if (gastosTotalesDelMes.Any())
        {
            decimal gastosActualesDelMes = gastosTotalesDelMes.Where(x => x.Id != gasto.GastoEncabezado.Id).Sum(x => x.MontoTotal);
            decimal gastosTotalesDelMesSegunTipoGasto = gastosActualesDelMes + gastoEncabezado.MontoTotal;

            return CrearMensajeExcedioPresupuesto(gastosTotalesDelMesSegunTipoGasto, presupuestoDelMes, tipoGasto.Nombre);
        }
        else
        {
            return CrearMensajeExcedioPresupuesto(gastoEncabezado.MontoTotal, presupuestoDelMes, tipoGasto.Nombre);
        }
    }

    private StringBuilder CrearMensajeExcedioPresupuesto(decimal gastoTotalMes, decimal presupuestoDelMes, string nombreTipoGasto)
    {
        StringBuilder stringBuilder = new();
        decimal montoExcedente = 0;

        if (gastoTotalMes == presupuestoDelMes)
            stringBuilder.Append($"El saldo de {nombreTipoGasto} esta en 0, considerelo para los futuros gastos a agregar");

        if(gastoTotalMes > presupuestoDelMes)
        {
            montoExcedente = gastoTotalMes - presupuestoDelMes;
            stringBuilder.Append($"<b>{nombreTipoGasto}:</b> Presupuestado {StringUtil.ConvertToMoneyFormat(presupuestoDelMes)}<br>Gastado {gastoTotalMes}<br>Se ha pasado por {StringUtil.ConvertToMoneyFormat(montoExcedente)}<br>");
        }

        return stringBuilder;
    }

    private async Task<(decimal, string)> ActualizarGastoEncabezado(GastoDto gasto, GastoEncabezado gastoEncabezado, decimal fondoMonetarioSaldoActual)
    {
        GastoEncabezado gastoEncabezadoAActualizar = (await gastoEncabezadoRepository.GetAsyncInclude(x => x.Id == gasto.GastoEncabezado.Id, x => x.Include(x => x.Detalles)))!;

        decimal diferenciaMontos = gastoEncabezadoAActualizar.MontoTotal - gastoEncabezado.MontoTotal;

        decimal nuevoSaldoFondoMonetario = fondoMonetarioSaldoActual + diferenciaMontos;

        gastoEncabezadoAActualizar.Fecha = gastoEncabezado.Fecha;
        gastoEncabezadoAActualizar.FondoMonetarioId = gastoEncabezado.FondoMonetarioId;
        gastoEncabezadoAActualizar.Observaciones = gastoEncabezado.Observaciones;
        gastoEncabezadoAActualizar.NombreComercio = gastoEncabezado.NombreComercio;
        gastoEncabezadoAActualizar.TipoDocumento = gastoEncabezado.TipoDocumento;
        gastoEncabezadoAActualizar.NumeroDocumento = gastoEncabezado.NumeroDocumento;
        gastoEncabezadoAActualizar.MontoTotal = gastoEncabezado.MontoTotal;
        gastoEncabezadoAActualizar.Detalles = gastoEncabezado.Detalles;

        await gastoEncabezadoRepository.UpdateAsync(gastoEncabezadoAActualizar);
        
        return (nuevoSaldoFondoMonetario, "Se ha actualizado correctamente el gasto.");
    }

    private async Task<(decimal, string)> CrearGastoEncabezado(GastoDto gasto, GastoEncabezado gastoEncabezado, decimal fondoMonetarioSaldoActual)
    {
        gastoEncabezado.UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"); //TODO cambiar cuando se implemente el identity
        await gastoEncabezadoRepository.AddAsync(gastoEncabezado);

        return (fondoMonetarioSaldoActual - gastoEncabezado.MontoTotal, "Se ha creado correctamente el gasto.");
    }
}
