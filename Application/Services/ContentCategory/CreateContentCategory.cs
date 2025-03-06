using Application.Services.Base;
using Application.Services.ContentCategory.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.Services.Employee;
using Application.UnitOfWorks;

namespace Application.Services.ContentCategory;

public class CreateContentCategory(IServiceProvider serviceProvider)
    : BaseSvc<ContentCategoryDto, ContentCategoryDto>(serviceProvider)
{
    protected override async Task<ContentCategoryDto> _InvokeAsync(GenericUoW uow, ContentCategoryDto request)
    {
        var newContentCategory = new Data.Entities.ContentCategory
        {
            ParentId = request.ParentId,
            Title = request.Title,
            Icon = request.Icon,
            Order = request.Order,
            IsActive = request.IsActive ?? true
        };

        await uow.Repository<Data.Entities.ContentCategory>()
            .AddAsync(newContentCategory);
        await uow.SaveChangesAsync();
        
        return newContentCategory.ToDto();
    }
}