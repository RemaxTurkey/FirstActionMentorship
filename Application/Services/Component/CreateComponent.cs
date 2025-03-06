using Application.Exceptions;
using Application.Extensions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.Services.Component.Extensions;
using Application.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class CreateComponent(IServiceProvider serviceProvider)
    : BaseSvc<CreateComponent.Request, CreateComponent.Response>(serviceProvider)
{
    public class Request : ComponentDto;
    
    public class Response
    {
        public ComponentDto Item { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        var componentType = await uow.Repository<Data.Entities.ComponentType>()
            .GetByIdAsync(request.ComponentTypeId);
        
        if (componentType == null)
            throw new BusinessException($"ComponentType with ID {request.ComponentTypeId} not found.");
        
        var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
            new GetComponentTypeAttributes.Request { ComponentTypeId = request.ComponentTypeId });
        
        var validAttributeIds = typeAttributesResponse.Attributes.Select(a => a.Id.Value).ToList();
        
        if (request.ComponentItems != null && request.ComponentItems.Any())
        {
            foreach (var item in request.ComponentItems)
            {
                if (!validAttributeIds.Contains(item.AttributeId))
                {
                    throw new BusinessException($"Attribute with ID {item.AttributeId} is not associated with ComponentType {request.ComponentTypeId}");
                }
            }
        }
        
        if (request.ComponentAttributes != null && request.ComponentAttributes.Any())
        {
            foreach (var attr in request.ComponentAttributes)
            {
                if (!validAttributeIds.Contains(attr.ComponentTypeAttributeId))
                {
                    throw new BusinessException($"Attribute with ID {attr.ComponentTypeAttributeId} is not associated with ComponentType {request.ComponentTypeId}");
                }
            }
        }
        
        var newComponent = new Data.Entities.Component
        {
            ComponentTypeId = request.ComponentTypeId,
            IsActive = request.IsActive
        };
        
        await uow.Repository<Data.Entities.Component>()
            .AddAsync(newComponent);
        await uow.SaveChangesAsync();
        
        if (request.ComponentItems != null && request.ComponentItems.Any())
        {
            foreach (var itemDto in request.ComponentItems)
            {
                await Svc<AddComponentItem>().InvokeAsync(uow, new AddComponentItem.Request
                {
                    ComponentId = newComponent.Id,
                    ComponentTypeId = request.ComponentTypeId,
                    Item = itemDto
                });
            }
        }
        
        if (request.ComponentAttributes != null && request.ComponentAttributes.Any())
        {
            foreach (var attrDto in request.ComponentAttributes)
            {
                await Svc<AddComponentAttribute>().InvokeAsync(uow, new AddComponentAttribute.Request
                {
                    ComponentId = newComponent.Id,
                    ComponentTypeId = request.ComponentTypeId,
                    Attribute = attrDto
                });
            }
        }
        
        var createdComponent = await uow.Repository<Data.Entities.Component>()
            .GetByIdAsync(newComponent.Id, "ComponentItems,ComponentAttributes.ComponentTypeAttribute");
        
        return new Response { Item = createdComponent.ToDto() };
    }
}