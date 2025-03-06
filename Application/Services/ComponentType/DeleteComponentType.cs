using Application.Exceptions;
using Application.Services.Base;
using Application.UnitOfWorks;

namespace Application.Services.ComponentType;

public class DeleteComponentType(IServiceProvider serviceProvider) 
    : BaseSvc<DeleteComponentType.Request, DeleteComponentType.Response>(serviceProvider)
    {
        public record Request(int Id);
        public record Response(int Id);

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var componentType = await uow.Repository<Data.Entities.ComponentType>().GetByIdAsync(req.Id);
            
            if (componentType == null)
                throw new BusinessException("Component type not found");

            uow.Repository<Data.Entities.ComponentType>().Delete(componentType);
            await uow.SaveChangesAsync();

            return new Response(componentType.Id);
        }
} 