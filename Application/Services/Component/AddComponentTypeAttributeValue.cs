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
            existingAttribute.IsActive = true;
            uow.Repository<Data.Entities.ComponentAttributeValue>().Update(existingAttribute);
        }
        else
        {
            var componentAttribute = new Data.Entities.ComponentAttributeValue
            {
                ComponentId = request.ComponentId,
                ComponentTypeAttributeId = request.AttributeValue.ComponentTypeAttributeId,
                Value = request.AttributeValue.Value,
                IsActive = true
            };

            await uow.Repository<Data.Entities.ComponentAttributeValue>().AddAsync(componentAttribute);
        }

        await uow.SaveChangesAsync();

        return new Response { Success = true };
    }

    private static async Task ValidateComponentTypeRules(GenericUoW uow, Request request, Data.Entities.Component component)
    {
        if (component == null)
            {
                throw new BusinessException($"Component with ID {request.ComponentId} not found.");
            }
    }
} 