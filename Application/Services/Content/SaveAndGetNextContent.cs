using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content;

public class SaveAndGetNextContent : BaseSvc<SaveAndGetNextContent.Request, SaveAndGetNextContent.Response>
{
    public SaveAndGetNextContent(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var contentEmployeeAssoc = await uow.Repository<ContentEmployeeAssoc>()
            .FindBy(x => x.ContentId == req.NextContentId && x.EmployeeId == req.EmployeeId)
            .FirstOrDefaultAsync();

        if (contentEmployeeAssoc != null)
        {
            var recordIsExist = await uow.Repository<ContentEmployeeRecord>()
                .FindByNoTracking(x => x.ContentId == req.SaveContentId 
                    && x.ContentEmployeeAssocId == contentEmployeeAssoc.Id)
                .FirstOrDefaultAsync();

            if (recordIsExist == null)
            {
                var contentEmployeeRecord = new ContentEmployeeRecord
                {
                    ContentId = req.SaveContentId,
                    CompletionDate = DateTime.Now,
                    ContentEmployeeAssocId = contentEmployeeAssoc.Id,
                    EmployeeId = req.EmployeeId,
                    IsActive = true
                };

                await uow.Repository<ContentEmployeeRecord>().AddAsync(contentEmployeeRecord);
                await uow.SaveChangesAsync();
            }
            
            var parentContentId = await uow.Repository<Data.Entities.Content>()
                .FindByNoTracking(x => x.Id == req.SaveContentId)
                .Select(x => x.ParentId)
                .FirstOrDefaultAsync();

            if (parentContentId.HasValue)
            {
                await Svc<CheckContentCompletion>().InvokeAsync(
                    uow,
                    new CheckContentCompletion.Request(parentContentId.Value, req.EmployeeId));
            }
        }

        var nextContent = await Svc<GetContent>().InvokeAsync(
            uow,
            new GetContent.Request(req.NextContentId, req.EmployeeId));

        return new Response
        {
            Item = nextContent.Item
        };
    }

    public record Request(int SaveContentId, int NextContentId, int EmployeeId);

    public class Response
    {
        public GetContent.ContentDetailViewModel Item { get; set; }
    }
} 