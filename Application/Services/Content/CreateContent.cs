using System.Diagnostics;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.Services.Content.DTOs;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Application.Services.Content;

public class CreateContent : BaseSvc<CreateContent.Request, CreateContent.Response>
{
    public CreateContent(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        ValidateComponents(req);

        var content = new Data.Entities.Content
        {
            Header = req.Header,
            IsActive = req.IsActive,
            CreatedDate = DateTime.Now,
            ParentId = req.ParentId,
        };
        
        await uow.Repository<ContentComponentAssoc>()
            .AddRangeAsync(req.Components!.Select(x =>
                new ContentComponentAssoc()
                {
                    ComponentId = x.Id!.Value,
                    IsActive = x.IsActive,
                    Content = content,
                    Order = x.Order!.Value})
                .ToList());

        await uow.SaveChangesAsync();

        return new();
    }

    private void ValidateComponents(Request request)
    {
        if (request.Components is null)
        {
            throw new BusinessException("Components are required");
        }

        if (request.Components.Any(x => x.Order is null))
        {
            throw new BusinessException("Order is required");
        }
    }

    public class Request : ContentDto;

    public class Response
    {
        public ContentDto Item { get; set; }
    }
}