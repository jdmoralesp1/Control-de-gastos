using PruebaTecnicaMVC.Modelos.DTOs;

namespace PruebaTecnicaMVC.Aplicacion.Services.Contracts;
public interface IGastoService
{
    public Task<List<(string, string)>> Upsert(GastoDto gasto);
}
