using Application.Services.Base;
using Application.Services.ContentCategory.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ContentCategory;

public class UpdateContentCategory(IServiceProvider serviceProvider)
    : BaseSvc<UpdateContentCategory.Request, UpdateContentCategory.Response>(serviceProvider)
{
    public class Request : ContentCategoryDto;

    public class Response
    {
        public ContentCategoryDto Item { get; set; }
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        ArgumentNullException.ThrowIfNull(req);

        var contentCategory = await uow.Repository<Data.Entities.ContentCategory>()
            .FindBy(x => x.Id == req.Id)
            .FirstOrDefaultAsync();

        ArgumentNullException.ThrowIfNull(contentCategory);

        contentCategory.Title = req.Title;
        contentCategory.ParentId = req.ParentId;
        contentCategory.IsActive = req.IsActive ?? true;
        contentCategory.Icon = req.Icon;
        contentCategory.Order = req.Order;

        uow.Repository<Data.Entities.ContentCategory>().Update(contentCategory);
        await uow.SaveChangesAsync();

        return new Response { Item = contentCategory.ToDto() };
    }
}