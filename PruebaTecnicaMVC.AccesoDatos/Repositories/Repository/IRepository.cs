using Ardalis.Specification;
using System.Linq.Expressions;

namespace PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;

public interface IRepository<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    public void AddRange(List<TEntity> entities, CancellationToken cancellationToken = default);
    public void Delete(TEntity entity);
    public void DeleteRange(List<TEntity> entities);
    public IEnumerable<TEntity> GetAll();
    public Task<IEnumerable<TEntity>> GetAsync();
    public Task<IEnumerable<TEntity>> GetAsync
        (Expression<Func<TEntity, bool>>? whereCondition = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "");
    public Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? condicionWhere);
    public Task<TEntity?> GetLastBy<TKey>(Expression<Func<TEntity, TKey>> orderBySelector);
    public Task<TEntity?> GetAsyncInclude(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null);
    public Task<IEnumerable<TEntity>> GetAllAsyncInclude(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null);
    public Task<TEntity?> GetById(int id);
    public IEnumerable<TEntity> GetAsyncAsNoTracking(Expression<Func<TEntity, bool>>? whereCondition = null);

}
