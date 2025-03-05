using Application.Repositories;
using Data.DbContexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitOfWorks;

public class GenericUoW
{
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;

    public GenericUoW(AppDbContext dbContext, bool enableTrace)
    {
        //
        DbContext = dbContext;

        DbContext.ChangeTracker.QueryTrackingBehavior = enableTrace
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }

    public GenericUoW(IServiceProvider serviceProvider, bool enableTrace)
        : this(serviceProvider.GetService<AppDbContext>()!, enableTrace)
    {
    }

    public GenericUoW(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public AppDbContext DbContext { get; set; }

    public virtual IRepository<T> Repository<T>() where T : Entity
    {
        if (_repositories.ContainsKey(typeof(T))) return (_repositories[typeof(T)] as IRepository<T>)!;

        IRepository<T> repository = new Repository<T>(DbContext);
        _repositories.Add(typeof(T), repository);
        return repository;
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    public virtual IDbContextTransaction BeginTransaction()
    {
        return DbContext.Database.BeginTransaction();
    }

    public virtual async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await DbContext.Database.BeginTransactionAsync();
    }

    public virtual async Task Commit()
    {
        await DbContext.Database.CommitTransactionAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
                DbContext.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}