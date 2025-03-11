using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.UnitOfWorks;
using Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class AddComponentItem(IServiceProvider serviceProvider)
    : BaseSvc<AddComponentItem.Request, AddComponentItem.Response>(serviceProvider)
{
    public class Request
    {
        public int ComponentId { get; set; }
        public int ComponentTypeId { get; set; }
        public ComponentItemDto Item { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    public class Response
    {
        public bool Success { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        var component = await uow.Repository<Data.Entities.Component>()
            .GetByIdAsync(request.ComponentId);
        
        if (component == null)
            throw new BusinessException($"Component with ID {request.ComponentId} not found.");
        
        var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
            new GetComponentTypeAttributes.Request { ComponentTypeId = request.ComponentTypeId });
        
        var validAttributeIds = typeAttributesResponse.Attributes.Select(a => a.Id.Value).ToList();
        
        if (!validAttributeIds.Contains(request.Item.AttributeId))
        {
            throw new BusinessException($"Attribute with ID {request.Item.AttributeId} is not associated with ComponentType {request.ComponentTypeId}");
        }
        
        var componentItem = new ComponentItem
        {
            ComponentId = request.ComponentId,
            ComponentTypeAttributeId = request.Item.AttributeId,
            Value = request.Item.Value,
            IsActive = request.IsActive
        };
        
        await uow.Repository<ComponentItem>()
            .AddAsync(componentItem);
        await uow.SaveChangesAsync();
        
        return new Response { Success = true };
    }
} 