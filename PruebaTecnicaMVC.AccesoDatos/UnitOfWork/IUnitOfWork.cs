namespace PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
public interface IUnitOfWork : IDisposable
{
    public Task BeginTransaction();
    public Task Rollback();
    public Task<int> Save();
    public void Dispose();

}
