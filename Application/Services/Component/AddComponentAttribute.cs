using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.UnitOfWorks;
using System;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class AddComponentTypeAttributeValue(IServiceProvider serviceProvider)
    : BaseSvc<AddComponentTypeAttributeValue.Request, AddComponentTypeAttributeValue.Response>(serviceProvider)
{
    public class Request
    {
        public int ComponentId { get; set; }
        public int ComponentTypeId { get; set; }
        public ComponentTypeAttributeValueDto AttributeValue { get; set; }
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

        await ValidateComponentTypeRules(uow, request, component);

        var existingAttribute = await uow.Repository<Data.Entities.ComponentAttributeValue>()
            .GetAsync(a =>
                a.ComponentId == request.ComponentId &&
                a.ComponentTypeAttributeId == request.AttributeValue.ComponentTypeAttributeId);

        if (existingAttribute != null)
        {
            existingAttribute.Value = request.AttributeValue.Value;
            existingAttribute.IsActive = request.IsActive;
            uow.Repository<Data.Entities.ComponentAttributeValue>().Update(existingAttribute);
        }
        else
        {
            var componentAttribute = new Data.Entities.ComponentAttributeValue
            {
                ComponentId = request.ComponentId,
                ComponentTypeAttributeId = request.AttributeValue.ComponentTypeAttributeId,
                Value = request.AttributeValue.Value,
                IsActive = request.IsActive
            };

            await uow.Repository<Data.Entities.ComponentAttributeValue>().AddAsync(componentAttribute);
        }

        await uow.SaveChangesAsync();

        return new Response { Success = true };
    }

    private async Task ValidateComponentTypeRules(GenericUoW uow, Request request, Data.Entities.Component component)
    {
        if (component == null)
            throw new BusinessException($"Component with ID {request.ComponentId} not found.");

        if (component.ComponentTypeId != request.ComponentTypeId)
            throw new BusinessException($"Component with ID {request.ComponentId} is not of type {request.ComponentTypeId}.");

        var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow,
            new GetComponentTypeAttributes.Request(request.ComponentTypeId));

        var validAttributeIds = typeAttributesResponse.Attributes.Select(a => a.Id.Value).ToList();

        if (!validAttributeIds.Contains(request.AttributeValue.ComponentTypeAttributeId))
        {
            throw new BusinessException($"Attribute with ID {request.AttributeValue.ComponentTypeAttributeId} is not associated with ComponentType {request.ComponentTypeId}");
        }
    }
} 