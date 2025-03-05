using Microsoft.EntityFrameworkCore;

namespace Application.Extensions;

public static class EnumerableExtension
{
    public static IQueryable<T> IncludeAll<T>(this IQueryable<T> queryable, params string[] includeParams)
        where T : class
    {
        foreach (var includeParam in includeParams)
        {
            queryable = queryable.Include(includeParam);
        }
        
        return queryable;
    }
}