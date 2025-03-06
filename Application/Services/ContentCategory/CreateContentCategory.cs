using Application.Exceptions;
using Application.Extensions;
using Application.Services.Base;
using Application.Services.ContentCategory.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.Services.Employee;
using Application.UnitOfWorks;

namespace Application.Services.ContentCategory;

public class CreateContentCategory(IServiceProvider serviceProvider)
    : BaseSvc<CreateContentCategory.Request, CreateContentCategory.Response>(serviceProvider)
{
    public class Request : ContentCategoryDto;
    
    public class Response
    {
        public ContentCategoryDto Item { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        ArgumentException.ThrowIfNullOrEmpty(request.Title);

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

        return new Response { Item = newContentCategory.ToDto() };
    }
}