using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.Services.Component.Extensions;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class UpdateComponent(IServiceProvider serviceProvider)
    : BaseSvc<UpdateComponent.Request, UpdateComponent.Response>(serviceProvider)
{
    public class Request
    {
        public int Id { get; set; }
        public int ComponentTypeId { get; set; }
        public List<ComponentItemDto> Items { get; set; }
        public List<ComponentTypeAttributeValueDto> ComponentTypeAttributeValues { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    public class Response
    {
        public ComponentDto Item { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        var component = await uow.Repository<Data.Entities.Component>()
            .FindBy(c => c.Id == request.Id)
            .FirstOrDefaultAsync();
        
        if (component == null)
            throw new BusinessException($"Component with ID {request.Id} not found.");
        
        var componentType = await uow.Repository<Data.Entities.ComponentType>()
            .GetByIdAsync(request.ComponentTypeId);
        
        if (componentType == null)
            throw new BusinessException($"ComponentType with ID {request.ComponentTypeId} not found.");
        
        var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
            new GetComponentTypeAttributes.Request(request.ComponentTypeId));
        
        var validAttributeIds = typeAttributesResponse.Attributes.Select(a => a.Id.Value).ToList();
        
        ValidateComponent(request, validAttributeIds);

        component.ComponentTypeId = request.ComponentTypeId;
        component.IsActive = request.IsActive;
        
        uow.Repository<Data.Entities.Component>().Update(component);
        await uow.SaveChangesAsync();
        
        if (request.ComponentTypeAttributeValues != null && request.ComponentTypeAttributeValues.Any())
        {
            var existingAttributeValues = await uow.Repository<ComponentAttributeValue>()
                .FindBy(cav => cav.ComponentId == component.Id && cav.IsActive)
                .ToListAsync();
            
            foreach (var attrDto in request.ComponentTypeAttributeValues)
            {
                if (!validAttributeIds.Contains(attrDto.ComponentTypeAttributeId))
                {
                    throw new BusinessException($"Attribute with ID {attrDto.ComponentTypeAttributeId} is not associated with ComponentType {request.ComponentTypeId}");
                }
                
                await Svc<AddComponentTypeAttributeValue>().InvokeAsync(uow, new AddComponentTypeAttributeValue.Request
                {
                    ComponentId = component.Id,
                    AttributeValue = attrDto,
                    IsActive = true
                });
            }
            
            var requestAttrIds = request.ComponentTypeAttributeValues
                .Select(a => a.ComponentTypeAttributeId)
                .ToList();
            
            foreach (var existingAttr in existingAttributeValues)
            {
                if (!requestAttrIds.Contains(existingAttr.ComponentTypeAttributeId))
                {
                    existingAttr.IsActive = false;
                    uow.Repository<ComponentAttributeValue>().Update(existingAttr);
                }
            }
        }
        
        var updatedComponent = await uow.Repository<Data.Entities.Component>()
            .FindBy(c => c.Id == component.Id)
            .Include(c => c.ComponentType)
            .Include(c => c.ComponentItems)
            .Include(c => c.ComponentAttributeValue)
                .ThenInclude(cav => cav.ComponentTypeAttribute)
            .FirstOrDefaultAsync();
        
        return new Response { Item = updatedComponent.ToDto() };
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