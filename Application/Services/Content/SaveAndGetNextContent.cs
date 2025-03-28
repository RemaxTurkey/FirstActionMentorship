using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content;

public class SaveContent : BaseSvc<SaveContent.Request, SaveContent.Response>
{
    public SaveContent(IServiceProvider serviceProvider) : base(serviceProvider)
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
                    && x.ContentEmployeeAssocId == contentEmployeeAssoc.Id
                    && x.PropertyId == req.PropertyId)
                .AnyAsync();

            if (!recordIsExist)
            {
                var contentEmployeeRecord = new ContentEmployeeRecord
                {
                    ContentId = req.SaveContentId,
                    CompletionDate = DateTime.Now,
                    ContentEmployeeAssocId = contentEmployeeAssoc.Id,
                    EmployeeId = req.EmployeeId,
                    PropertyId = req.PropertyId,
                    IsActive = true
                };

                await uow.Repository<ContentEmployeeRecord>().AddAsync(contentEmployeeRecord);
                await uow.SaveChangesAsync();
            }

            var parentContentId = await uow.Repository<Data.Entities.Content>()
                .FindByNoTracking(x => x.Id == req.SaveContentId)
                .Select(x => x.ParentId)
                .FirstOrDefaultAsync();

            if (parentContentId == Constants.Constants.PazarlamaContentId)
            {
                await Svc<CheckPazarlamaContentCompletion>().InvokeAsync(
                    uow,
                    new CheckPazarlamaContentCompletion.Request(req.EmployeeId, req.PropertyId.Value));
            }
            else if (parentContentId.HasValue)
            {
                await Svc<CheckContentCompletion>().InvokeAsync(
                    uow,
                    new CheckContentCompletion.Request(parentContentId.Value, req.EmployeeId, req.PropertyId));
            }
        }

        return new Response
        {
            Item = true
        };
    }

    public record Request(int SaveContentId, int NextContentId, int EmployeeId, int? PropertyId = null);

    public class Response
    {
        public bool Item { get; set; }
    }
}