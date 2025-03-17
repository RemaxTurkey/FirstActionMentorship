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
            new GetComponentTypeAttributes.Request(request.ComponentTypeId));
        
        var validAttributeIds = typeAttributesResponse.Attributes.Select(a => a.Id.Value).ToList();
        
        ValidateComponent(request, validAttributeIds);

        var newComponent = new Data.Entities.Component
        {
            ComponentTypeId = request.ComponentTypeId,
            IsActive = request.IsActive
        };
        
        await uow.Repository<Data.Entities.Component>()
            .AddAsync(newComponent);
        await uow.SaveChangesAsync();
        
        if (request.Items != null && request.Items.Any())
        {
            foreach (var itemDto in request.Items)
            {
                await Svc<AddComponentItem>().InvokeAsync(uow, new AddComponentItem.Request
                {
                    ComponentId = newComponent.Id,
                    ComponentTypeId = request.ComponentTypeId,
                    Item = itemDto,
                    IsActive = request.IsActive
                });
            }
        }
        
        if (request.ComponentTypeAttributeValues != null && request.ComponentTypeAttributeValues.Count != 0)
        {
            foreach (var attrDto in request.ComponentTypeAttributeValues)
            {
                await Svc<AddComponentTypeAttributeValue>().InvokeAsync(uow, new AddComponentTypeAttributeValue.Request
                {
                    ComponentId = newComponent.Id,
                    AttributeValue = attrDto,
                    IsActive = request.IsActive
                });
            }
        }
        
        var createdComponent = await uow.Repository<Data.Entities.Component>()
            .GetByIdAsync(newComponent.Id);
        
        return new Response { Item = createdComponent.ToDto() };
    }

    private static void ValidateComponent(Request request, List<int> validAttributeIds)
    {
        if (request.Items != null && request.Items.Any())
        {
            foreach (var item in request.Items)
            {
                if (!validAttributeIds.Contains(item.AttributeId))
                {
                    throw new BusinessException($"Attribute with ID {item.AttributeId} is not associated with ComponentType {request.ComponentTypeId}");
                }
            }
        }

        if (request.ComponentTypeAttributeValues != null && request.ComponentTypeAttributeValues.Any())
        {
            foreach (var attr in request.ComponentTypeAttributeValues)
            {
                if (!validAttributeIds.Contains(attr.ComponentTypeAttributeId))
                {
                    throw new BusinessException($"Attribute with ID {attr.ComponentTypeAttributeId} is not associated with ComponentType {request.ComponentTypeId}");
                }
            }
        }
    }
}