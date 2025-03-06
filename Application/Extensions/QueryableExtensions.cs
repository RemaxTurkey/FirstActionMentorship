using System.Linq.Expressions;
using System.Reflection;

namespace Application.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyOrder<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool isAscending)
    {
        var methodName = isAscending ? "OrderBy" : "OrderByDescending";
        var methodCallExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), typeof(TKey) },
            query.Expression,
            Expression.Quote(keySelector));

        return query.Provider.CreateQuery<T>(methodCallExpression);
    }

    public static IQueryable<T> ApplyOrder<T>(this IQueryable<T> query, string propertyName, bool isAscending)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName, 
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        
        if (propertyInfo == null)
            throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).Name}");

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyInfo);
        var lambda = Expression.Lambda(property, parameter);

        // Convert string-based property selection to expression and use the other overload
        var genericMethod = typeof(QueryableExtensions)
            .GetMethod(nameof(ApplyOrder), new[] { typeof(IQueryable<T>), typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType)), typeof(bool) });

        return (IQueryable<T>)genericMethod
            .MakeGenericMethod(typeof(T), propertyInfo.PropertyType)
            .Invoke(null, new object[] { query, lambda, isAscending });
    }
} 