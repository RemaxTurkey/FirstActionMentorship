using System.Diagnostics;
using System.Reflection;
using Application.UnitOfWorks;
using Application.Anotations;
using Microsoft.Extensions.Logging;
using Application.Attributes;
using Application.RedisCache;
using Application.Extensions;
using System.Collections.Concurrent;

namespace Application.Services.Base
{
    [Injectable]
    public abstract class BaseSvc<TReq, TResp> : AbstractService, ISvc<TReq, TResp>
        where TReq : class
        where TResp : class
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        
        protected readonly ICacheManager CacheManager;
        protected readonly ILogger<BaseSvc<TReq, TResp>> Logger;
        
        public BaseSvc(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            CacheManager = serviceProvider.GetService(typeof(ICacheManager)) as ICacheManager;
            Logger = serviceProvider.GetService(typeof(ILogger<BaseSvc<TReq, TResp>>)) as ILogger<BaseSvc<TReq, TResp>>;
        }

        protected abstract Task<TResp> _InvokeAsync(GenericUoW uow, TReq req);
        
        public virtual async Task<TResp> InvokeAsync(TReq request)
        {
            return await _InvokeAsync(request, NewUnitOfWork());
        }

        public virtual async Task<TResp> InvokeAsync()
        {
            return await InvokeAsync(null);
        }

        public virtual async Task<TResp> InvokeNoTrackingAsync(TReq request = null)
        {
            return await InvokeAsync(Svc<GenericUoW>(), request);
        }

        private async Task<TResp> _InvokeAsync(TReq req, GenericUoW uow)
        {
            await using var tx = await uow.BeginTransactionAsync();
            try
            {
                var resp = await InvokeAsync(uow, req);
                await tx.CommitAsync();
                return resp;
            }
            catch (Exception e)
            {
                await tx.RollbackAsync();
                Console.WriteLine(e);
                throw;
            }
        }
        
        private MethodInfo GetInvokeAsyncMethod()
        {
            return _methodCache.GetOrAdd(GetType(), type => 
                type.GetMethod("_InvokeAsync", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        internal virtual async Task<TResp> InvokeAsync(GenericUoW uow, TReq req = null)
        {
            var watch = new Stopwatch();
            watch.Start();

            var method = GetInvokeAsyncMethod();
            
            var cacheAttr = method.GetCustomAttribute<CacheAttribute>();
            if (cacheAttr != null && CacheManager != null)
            {
                var cacheKey = BuildCacheKey(cacheAttr.KeyTemplate, req);
                watch.Stop();
                Logger.Log(LogLevel.Information, $">>>> {GetType().Name} Execution Time: {watch.ElapsedMilliseconds} ms.");
                return await CacheManager.GetAsync(
                    cacheKey,
                    cacheAttr.CacheTime,
                    async () => await _InvokeAsync(uow, req)
                );
            }

            var resp = await _InvokeAsync(uow, req);

            watch.Stop();

            Logger?.Log(watch.ElapsedMilliseconds >= 100 ? LogLevel.Information : LogLevel.Debug,
                $">>>> {GetType().Name} Execution Time: {watch.ElapsedMilliseconds} ms.");

            // Logger.Log(LogLevel.Debug, $">>>> {GetType().Name} Execution Time: {watch.ElapsedMilliseconds} ms.");
            
            return resp;
        }
        
        private string BuildCacheKey(string template, TReq request)
        {
            var result = template;
            
            if (request == null)
                return result;
            
            var properties = typeof(TReq).GetProperties();
            foreach (var prop in properties)
            {
                var placeholder = $"{{{prop.Name}}}";
                if (template.Contains(placeholder))
                {
                    var value = prop.GetValue(request)?.ToString() ?? "null";
                    result = result.Replace(placeholder, value);
                }
            }
            
            return result;
        }
    }
}

public interface ISvc<in TReq, TResp>
    where TReq : class
    where TResp : class
{
    public Task<TResp> InvokeAsync(TReq req);
}