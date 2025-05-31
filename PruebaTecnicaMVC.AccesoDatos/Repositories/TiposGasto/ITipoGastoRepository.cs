using PruebaTecnicaMVC.Modelos.Entities;

namespace PruebaTecnicaMVC.AccesoDatos.Repositories.TiposGasto;
public interface ITipoGastoRepository
{
    public List<TipoGasto> GetAllAsync(string userId);
}
