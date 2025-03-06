using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.UnitOfWorks;
using System;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class AddComponentAttribute(IServiceProvider serviceProvider)
    : BaseSvc<AddComponentAttribute.Request, AddComponentAttribute.Response>(serviceProvider)
{
    public class Request
    {
        public int ComponentId { get; set; }
        public int ComponentTypeId { get; set; }
        public ComponentAttributeDto Attribute { get; set; }
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
        
        // Componentin type'ını kontrol et
        if (component.ComponentTypeId != request.ComponentTypeId)
            throw new BusinessException($"Component with ID {request.ComponentId} is not of type {request.ComponentTypeId}.");
        
        // Bu attribute type'a atanmış mı kontrol et
        var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
            new GetComponentTypeAttributes.Request { ComponentTypeId = request.ComponentTypeId });
        
        var validAttributeIds = typeAttributesResponse.Attributes.Select(a => a.Id.Value).ToList();
        
        if (!validAttributeIds.Contains(request.Attribute.ComponentTypeAttributeId))
        {
            throw new BusinessException($"Attribute with ID {request.Attribute.ComponentTypeAttributeId} is not associated with ComponentType {request.ComponentTypeId}");
        }
        
        // Aynı attribute daha önce eklenmiş mi kontrol et
        var existingAttribute = await uow.Repository<Data.Entities.ComponentAttribute>()
            .GetAsync(a => 
                a.ComponentId == request.ComponentId && 
                a.ComponentTypeAttributeId == request.Attribute.ComponentTypeAttributeId);
        
        if (existingAttribute != null)
        {
            // Varsa güncelle
            existingAttribute.Value = request.Attribute.Value;
            uow.Repository<Data.Entities.ComponentAttribute>().Update(existingAttribute);
        }
        else
        {
            // Yoksa ekle
            var componentAttribute = new Data.Entities.ComponentAttribute
            {
                ComponentId = request.ComponentId,
                ComponentTypeAttributeId = request.Attribute.ComponentTypeAttributeId,
                Value = request.Attribute.Value
            };
            
            await uow.Repository<Data.Entities.ComponentAttribute>().AddAsync(componentAttribute);
        }
        
        await uow.SaveChangesAsync();
        
        return new Response { Success = true };
    }
} 