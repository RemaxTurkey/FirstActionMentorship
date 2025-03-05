using Application.UnitOfWorks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services.Base
{
public abstract class AbstractService
{
    protected readonly IServiceProvider ServiceProvider;
    
    public AbstractService()
    {
    }
    
    protected AbstractService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    protected TSvc Svc<TSvc>()
    {
        return ServiceProvider.GetService<TSvc>()!;
    }
    
    protected UnitOfWorks.GenericUoW NewUnitOfWork()
    {
        return Svc<GenericUoWCreator>().Create(true);
    }
}
}