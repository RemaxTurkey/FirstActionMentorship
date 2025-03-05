using Application.Anotations;
using Application.UnitOfWorks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection RegisterInjectableServices(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes()
                .Where(y => !y.IsAbstract)
                .Where(type => type.GetCustomAttributes(typeof(Injectable), true)
                    .Any()));
        
        foreach (var assembly in assemblies)
        {
            services.AddTransient(assembly);
        }
        
        return services;
    }
    
    public static IServiceCollection AddCommonService(this IServiceCollection services)
    {
        services.AddScoped<UnitOfWorks.GenericUoW>();
        services.AddScoped<GenericUoWCreator>();
        
        return services;
    }
}