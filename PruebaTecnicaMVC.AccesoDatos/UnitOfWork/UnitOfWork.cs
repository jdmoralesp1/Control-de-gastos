using PruebaTecnicaMVC.AccesoDatos.Data;

namespace PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
public class UnitOfWork : IUnitOfWork
{
    private readonly PruebaTecnicaDbContext pruebaTecnicaDbContext;

    public UnitOfWork(PruebaTecnicaDbContext pruebaTecnicaDbContext)
    {
        this.pruebaTecnicaDbContext = pruebaTecnicaDbContext;
    }

    public async Task BeginTransaction()
    {
        await pruebaTecnicaDbContext.Database.BeginTransactionAsync();
    }

    public async Task Rollback()
    {
        await pruebaTecnicaDbContext.Database.RollbackTransactionAsync();
    }

    public async Task<int> Save() => await pruebaTecnicaDbContext.SaveChangesAsync();

    public void Dispose()
    {
        pruebaTecnicaDbContext.Dispose();
    }
}
