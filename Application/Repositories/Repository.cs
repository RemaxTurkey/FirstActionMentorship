using System.Linq.Expressions;
using Application.Extensions;
using Data.DbContexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = _context.Set<TEntity>();
    }

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, params string[] includes)
    {
        return await _baseQuery(false, includes).FirstOrDefaultAsync(expression);
    }
    
    public async Task<TEntity> GetByIdAsync(int id, params string[] includes)
    {
        return await _baseQuery(false, includes).FirstOrDefaultAsync();
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        return (await _set.AddAsync(entity)).Entity;
    }

    public async Task AddRangeAsync(List<TEntity> entity)
    {
        await _set.AddRangeAsync(entity);
    }

    public void Update(TEntity entity)
    {
        _set.Update(entity);
    }


    public bool Delete(TEntity entity)
    {
        _set.Remove(entity);
        return true;
    }


    public IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> expression, params string[] includes)
    {
        return _baseQuery(true, includes).Where(expression);
    }


    public IQueryable<TEntity> FindByNoTracking(Expression<Func<TEntity, bool>> expression, params string[] includes)
    {
        return _baseQuery(true, includes).Where(expression);
    }

    private IQueryable<TEntity> _baseQuery(bool disableTracking, params string[] includesParams)
    {
        if (disableTracking)
        {
            return _set.AsNoTracking().IncludeAll(includesParams);
        }

        return _set.IncludeAll(includesParams);
    }
}