using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Content;

public class CheckContentCompletion : BaseSvc<CheckContentCompletion.Request, CheckContentCompletion.Response>
{
    public CheckContentCompletion(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var siblingContents = await uow.Repository<Data.Entities.Content>()
            .FindByNoTracking(x => x.ParentId == req.ParentContentId)
            .ToListAsync();

        var siblingContentIds = siblingContents.Select(x => x.Id).ToList();

        var contentRecords = await uow.Repository<ContentEmployeeRecord>()
            .FindByNoTracking(x => siblingContentIds.Contains(x.ContentId))
            .Include(x => x.ContentEmployeeAssoc)
            .Where(x => x.ContentEmployeeAssoc.EmployeeId == req.EmployeeId)
            .ToListAsync();

        var recordedContentIds = contentRecords.Select(x => x.ContentId).Distinct().ToList();

        bool allContentsCompleted = siblingContentIds.All(id => recordedContentIds.Contains(id));

        if (allContentsCompleted)
        {
            var parentAssoc = await uow.Repository<ContentEmployeeAssoc>()
                .FindBy(x => x.ContentId == req.ParentContentId && x.EmployeeId == req.EmployeeId)
                .FirstOrDefaultAsync();

            if (parentAssoc != null)
            {
                parentAssoc.IsCompleted = true;
                parentAssoc.CompletionDate = DateTime.Now;
                uow.Repository<ContentEmployeeAssoc>().Update(parentAssoc);
                await uow.SaveChangesAsync();
            }
        }

        return new Response
        {
            AllContentsCompleted = allContentsCompleted
        };
    }

    public record Request(int ParentContentId, int EmployeeId);

    public class Response
    {
        public bool AllContentsCompleted { get; set; }
    }
} 