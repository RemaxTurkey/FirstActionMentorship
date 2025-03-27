using Application.Services.Base;
using Application.Services.Employee;
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
            .FindByNoTracking(x => x.ParentId == req.ParentContentId 
                                   && x.PageType == PageType.Content
                                   && x.Id != Constants.Constants.PowerStartContentId)
            .ToListAsync();

        var siblingContentIds = siblingContents.Select(x => x.Id).ToList();

        var contentRecords = await uow.Repository<ContentEmployeeRecord>()
            .FindByNoTracking(x => siblingContentIds.Contains(x.ContentId))
            .Include(x => x.ContentEmployeeAssoc)
            .Where(x => x.ContentEmployeeAssoc.EmployeeId == req.EmployeeId)
            .ToListAsync();

        var recordedContentIds = contentRecords.Select(x => x.ContentId).Distinct().ToList();

        if (req.ParentContentId == Constants.Constants.ContentHazirlikId)
        {
            var photo = await Svc<CheckEmployeeContentAssociation>().InvokeNoTrackingAsync(
                new CheckEmployeeContentAssociation.Request(ContentId: 7, EmployeeId: req.EmployeeId, PageType.Static));

            if (!photo.Exists)
            {
                return new Response
                {
                    AllContentsCompleted = false
                };
            }

            var videoCv = await Svc<CheckEmployeeContentAssociation>().InvokeNoTrackingAsync(
                new CheckEmployeeContentAssociation.Request(ContentId: 9, EmployeeId: req.EmployeeId, PageType.Static));

            if (!videoCv.Exists)
            {
                return new Response
                {   
                    AllContentsCompleted = false
                };
            }
            
            var introduction = await Svc<CheckEmployeeContentAssociation>().InvokeNoTrackingAsync(
                new CheckEmployeeContentAssociation.Request(ContentId: 8, EmployeeId: req.EmployeeId, PageType.Static));

            if (!introduction.Exists)
            {
                return new Response
                {
                    AllContentsCompleted = false
                };
            }

            var socialMedia = await Svc<CheckEmployeeContentAssociation>().InvokeNoTrackingAsync(
                new CheckEmployeeContentAssociation.Request(ContentId: 10, EmployeeId: req.EmployeeId, PageType.Static));

            if (!socialMedia.Exists)
            {
                return new Response
                {
                    AllContentsCompleted = false
                };
            }
        }
        
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