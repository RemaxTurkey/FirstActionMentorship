using System.Diagnostics;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component;
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
            IsActive = true,
            CreatedDate = DateTime.Now,
            ParentId = req.ParentId,
            IsMenu = req.IsMenu
        };

        foreach (var component in req.Components)
        {
            var contentComponentAssoc = new ContentComponentAssoc()
            {
                Component = new Data.Entities.Component
                {
                    ComponentTypeId = component.ComponentTypeId,
                    IsActive = true
                },
                IsActive = true,
                Content = content,
                Order = component.Order
            };

            await uow.Repository<ContentComponentAssoc>().AddAsync(contentComponentAssoc);
            await uow.SaveChangesAsync();
            
            foreach(var attribute in component.AttributeValue)
            {
                await Svc<AddComponentTypeAttributeValue>().InvokeAsync(uow, new AddComponentTypeAttributeValue.Request{
                    ComponentId = contentComponentAssoc.ComponentId,
                    AttributeValue = new ComponentTypeAttributeValueDto
                    {
                        ComponentId = contentComponentAssoc.ComponentId,
                        ComponentTypeAttributeId = attribute.ComponentTypeAttributeId,
                        Value = attribute.Value
                    }
                });
            }
        
        }
        
        await uow.SaveChangesAsync();

        return new();
    }

    private void ValidateComponents(Request request)
    {
        if (request.Components is null)
        {
            throw new BusinessException("Components are required");
        }
    }

    public class Request
    {
        public int? ParentId { get; set; }
        public string Header { get; set; }
        public List<ContentCreationComponentDto> Components { get; set; }
        public bool IsMenu { get; set; }
    }
    
    public class ContentCreationComponentDto 
    {
        public int ComponentTypeId { get; set; }
        public int Order { get; set; }
        public List<ComponentItemDto> Items { get; set; }
        public List<ContentCreationComponentTypeAttributeValueDto> AttributeValue { get; set; } = new();
    }

    public class ContentCreationComponentTypeAttributeValueDto
    {
        public int ComponentTypeAttributeId { get; set; }
        public string Value { get; set; }
    }

    public class Response
    {
        public ContentDto Item { get; set; }
    }
}