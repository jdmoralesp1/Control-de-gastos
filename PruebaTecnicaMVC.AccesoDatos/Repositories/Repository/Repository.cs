using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaMVC.AccesoDatos.Data;
using System.Linq.Expressions;

namespace PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;

public class Repository<TEntity>(PruebaTecnicaDbContext _context) : RepositoryBase<TEntity>(_context), IRepository<TEntity> where TEntity : class
{
    public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public override Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);
        return Task.FromResult(entity);
    }

    public override Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public void AddRange(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().AddRange(entities);
    }

    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public void DeleteRange(List<TEntity> entities)
    {
        _context.Set<TEntity>().RemoveRange(entities.ToArray());
    }

    public IEnumerable<TEntity> GetAll()
    {
        return (IQueryable<TEntity>)_context.Set<TEntity>().ToList();
    }

    public async Task<IEnumerable<TEntity>> GetAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAsync
        (Expression<Func<TEntity, bool>>? whereCondition = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "")
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();


        if (whereCondition is not null)
        {
            query = query.Where(whereCondition);
        }

        foreach (string? includeProperty in includeProperties
            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return await orderBy(query).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? condicionWhere)
    {
        if (condicionWhere is null)
        {
            throw new ArgumentNullException(nameof(condicionWhere));
        }

        return await _context.Set<TEntity>().FirstOrDefaultAsync(condicionWhere);
    }

    public async Task<TEntity?> GetLastBy<TKey>(Expression<Func<TEntity, TKey>> orderBySelector)
    {
        return await _context.Set<TEntity>().OrderBy(orderBySelector).LastOrDefaultAsync();
    }

    public async Task<TEntity?> GetAsyncInclude(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (include != null)
        {
            query = include(query);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsyncInclude(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (include != null)
        {
            query = include(query);
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity?> GetById(int id)
    {
        return await _context.Set<TEntity>().FindAsync(id)!;
    }
    public IQueryable<TEntity> Query()
    {
        return _context.Set<TEntity>().AsQueryable();
    }
}
