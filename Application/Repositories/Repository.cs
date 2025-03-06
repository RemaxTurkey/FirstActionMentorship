using System.Linq.Expressions;
using Application.Extensions;
using Application.Models;
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

    public IQueryable<TEntity> GetAll(params string[] includes)
    {
        return _baseQuery(true, includes);
    }

    public async Task<PagedResult<TEntity>> GetAllPagedAsync(PaginationModel pagination, params string[] includes)
    {
        var query = _baseQuery(true, includes);
        return await GetPagedResultAsync(query, pagination);
    }

    public async Task<PagedResult<TEntity>> FindNoTrackingByPagedAsync(Expression<Func<TEntity, bool>> expression, PaginationModel pagination, params string[] includes)
    {
        var query = _baseQuery(true, includes).Where(expression);
        return await GetPagedResultAsync(query, pagination);
    }

    private async Task<PagedResult<TEntity>> GetPagedResultAsync(IQueryable<TEntity> query, PaginationModel pagination)
    {
        var totalCount = await query.CountAsync();
        
        if (!string.IsNullOrWhiteSpace(pagination.OrderBy))
        {
            query = query.ApplyOrder(pagination.OrderBy, pagination.IsAscending);
        }

        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<TEntity>(
            items, 
            totalCount, 
            pagination.PageSize, 
            pagination.PageNumber,
            pagination.OrderBy ?? "Id",
            pagination.IsAscending
        );
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