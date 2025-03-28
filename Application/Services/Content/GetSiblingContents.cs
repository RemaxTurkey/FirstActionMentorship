using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Content
{
    public class GetSiblingContents : BaseSvc<GetSiblingContents.Request, GetSiblingContents.Response>
    {
        public GetSiblingContents(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            if (req.ParentContentId == Constants.Constants.KomsuMektubuDagitParentContentId)
            {
                var siblings = await uow.Repository<Data.Entities.Content>()
                    .FindByNoTracking(x => x.ParentId == req.ParentContentId
                                           && (x.PageType == PageType.Content || x.Id == Constants.Constants.KomsuMektubuDagitContentId))
                    .ToListAsync();
                
                return new Response(siblings);
            }
            
            var siblingContents = await uow.Repository<Data.Entities.Content>()
                .FindByNoTracking(x => x.ParentId == req.ParentContentId
                                       && x.PageType == PageType.Content )
                .ToListAsync();

            return new Response(siblingContents);
        }

        public record Request(int ParentContentId, int EmployeeId, int? PropertyId = null);
        public record Response(List<Data.Entities.Content> Contents);
    }
}