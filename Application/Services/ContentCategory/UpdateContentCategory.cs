using Application.Services.Base;
using Application.Services.ContentCategory.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ContentCategory;

public class UpdateContentCategory(IServiceProvider serviceProvider)
    : BaseSvc<ContentCategoryDto, ContentCategoryDto>(serviceProvider)
{
    protected override async Task<ContentCategoryDto> _InvokeAsync(GenericUoW uow, ContentCategoryDto req)
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

        return contentCategory.ToDto();
    }
}