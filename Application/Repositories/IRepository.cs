using System.Linq.Expressions;
using Data.Entities;

namespace Application.Repositories;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, params string[] includes);
    Task<TEntity> GetByIdAsync(int id, params string[] includes);
    Task<TEntity> AddAsync(TEntity entity);
    Task AddRangeAsync(List<TEntity> entity);
    void Update(TEntity entity);
    bool Delete(TEntity entity);
    IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> expression, params string[] includes);
    IQueryable<TEntity> FindByNoTracking(Expression<Func<TEntity, bool>> expression, params string[] includes);
}