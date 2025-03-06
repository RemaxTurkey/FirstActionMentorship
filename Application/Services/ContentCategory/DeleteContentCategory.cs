using Application.Exceptions;
using Application.Services.Base;
using Application.UnitOfWorks;

namespace Application.Services.ContentCategory;

public class DeleteContentCategory(IServiceProvider serviceProvider) 
    : BaseSvc<DeleteContentCategory.Request, DeleteContentCategory.Response>(serviceProvider)
    {
        public record Request(int Id);
        public record Response(int Id);

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var contentCategory = await uow.Repository<Data.Entities.ContentCategory>().GetByIdAsync(req.Id);
            
            if (contentCategory == null)
                throw new BusinessException("Content category not found");

            uow.Repository<Data.Entities.ContentCategory>().Delete(contentCategory);
            await uow.SaveChangesAsync();

            return new Response(contentCategory.Id);
        }
}
