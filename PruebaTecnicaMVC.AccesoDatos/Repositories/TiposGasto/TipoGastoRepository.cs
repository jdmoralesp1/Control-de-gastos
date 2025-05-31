using PruebaTecnicaMVC.AccesoDatos.Data;
using PruebaTecnicaMVC.Modelos.Entities;

namespace PruebaTecnicaMVC.AccesoDatos.Repositories.TiposGasto;
public class TipoGastoRepository : ITipoGastoRepository
{
    private PruebaTecnicaDbContext pruebaTecnicaDbContext;

    public TipoGastoRepository(PruebaTecnicaDbContext pruebaTecnicaDbContext)
    {
        this.pruebaTecnicaDbContext = pruebaTecnicaDbContext;
    }

    public List<TipoGasto> GetAllAsync(string userId)
    {
        return pruebaTecnicaDbContext.TiposGasto
            .Where(x => x.Activo && x.UsuarioId == Guid.Parse(userId))
            .OrderBy(x => x.Id == 1 ? 0 : 1) // El Id 1 primero
            .ThenBy(x => x.Nombre)           // Luego el resto por nombre
            .ToList();
    }
}
