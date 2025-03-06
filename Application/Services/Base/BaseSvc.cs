using System.Diagnostics;
using System.Reflection;
using Application.UnitOfWorks;
using Application.Anotations;
using Microsoft.Extensions.Logging;

namespace Application.Services.Base
{
    [Injectable]
    public abstract class BaseSvc<TReq, TResp> : AbstractService, ISvc<TReq, TResp>
        where TReq : class
        where TResp : class
    {
        public BaseSvc(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

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
            return await _InvokeAsync(Svc<GenericUoW>(), request);
        }


        public virtual TResp InvokeNoTracking(TReq request = null)
        {
            try
            {
                return _InvokeAsync(Svc<GenericUoW>(), request).Result;
            }
            catch (AggregateException error)
            {
                throw error.InnerException ?? error;
            }
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

        public virtual TResp Invoke(GenericUoW uow, TReq req)
        {
            try
            {
                return _InvokeAsync(uow, req).Result;
            }
            catch (AggregateException error)
            {
                throw error.InnerException;
            }
        }

        internal virtual async Task<TResp> InvokeAsync(GenericUoW uow, TReq req = null)
        {
            var watch = new Stopwatch();
            watch.Start();


            var method = GetType().GetMethod("_InvokeAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            var resp = await _InvokeAsync(uow, req);

            watch.Stop();

            //Logger.Log(watch.ElapsedMilliseconds >= 100 ? LogLevel.Information : LogLevel.Debug,
            //    $">>>> {GetType().Name} Execution Time: {watch.ElapsedMilliseconds} ms.");

            return resp;
        }

        protected abstract Task<TResp> _InvokeAsync(GenericUoW uow, TReq req);
    }
}

public interface ISvc<in TReq, TResp>
    where TReq : class
    where TResp : class
{
    public Task<TResp> InvokeAsync(TReq req);
}