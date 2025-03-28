using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
namespace Application.Services.Content
{
    public class CheckPazarlamaContentCompletion : BaseSvc<CheckPazarlamaContentCompletion.Request, CheckPazarlamaContentCompletion.Response>
    {
        public CheckPazarlamaContentCompletion(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
        {
            var siblingContents = await Svc<GetSiblingContents>().InvokeAsync(
                uow,
                new GetSiblingContents.Request(Constants.Constants.PazarlamaContentId, req.EmployeeId, req.PropertyId));

            var siblingContentIds = siblingContents.Contents.Select(x => x.Id).ToList();
            
            var contentRecords = await uow.Repository<ContentEmployeeRecord>()
                .FindByNoTracking(x => siblingContentIds.Contains(x.ContentId))
                .Include(x => x.ContentEmployeeAssoc)
                .Where(x => x.ContentEmployeeAssoc.EmployeeId == req.EmployeeId)
                .ToListAsync();
                
            var recordsByProperty = contentRecords
                .GroupBy(x => x.PropertyId)
                .ToList();
            
            bool isCompleted = false;
            
            foreach (var propertyGroup in recordsByProperty)
            {
                var recordedContentIds = propertyGroup.Select(x => x.ContentId).Distinct().ToList();
                bool allContentsCompletedForProperty = siblingContentIds.All(id => recordedContentIds.Contains(id));
                
                if (allContentsCompletedForProperty)
                {
                    isCompleted = true;
                    
                    var parentAssoc = await uow.Repository<ContentEmployeeAssoc>()
                        .FindBy(x => x.ContentId == Constants.Constants.PazarlamaContentId && x.EmployeeId == req.EmployeeId)
                        .FirstOrDefaultAsync();

                    if (parentAssoc != null)
                    {
                        parentAssoc.IsCompleted = true;
                        parentAssoc.CompletionDate = DateTime.Now;
                        uow.Repository<ContentEmployeeAssoc>().Update(parentAssoc);
                        await uow.SaveChangesAsync();
                    }
                    
                    break;
                }
            }
            
            return new Response(isCompleted);
        }
        public record Request(int EmployeeId, int PropertyId);
        public record Response(bool IsCompleted);
    }
}