using Application.Exceptions;
using Application.Services.Base;
using Application.Services.ComponentTypeAttributeAssoc.DTOs;
using Application.UnitOfWorks;
using System;
using System.Threading.Tasks;

namespace Application.Services.ComponentTypeAttributeAssoc;

public class AssignAttributeToComponentType(IServiceProvider serviceProvider)
    : BaseSvc<AssignAttributeToComponentType.Request, AssignAttributeToComponentType.Response>(serviceProvider)
{
    public class Request : ComponentTypeAttributeAssocDto;
    
    public class Response
    {
        public bool Success { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        var componentType = await uow.Repository<Data.Entities.ComponentType>()
            .GetByIdAsync(request.ComponentTypeId);
        
        if (componentType == null)
            throw new BusinessException($"ComponentType with ID {request.ComponentTypeId} not found.");
        
        var attribute = await uow.Repository<Data.Entities.ComponentTypeAttribute>()
            .GetByIdAsync(request.ComponentTypeAttributeId);
        
        if (attribute == null)
            throw new BusinessException($"ComponentTypeAttribute with ID {request.ComponentTypeAttributeId} not found.");
        
        var existingAssoc = await uow.Repository<Data.Entities.ComponentTypeAttributeAssoc>()
            .GetAsync(a => 
                a.ComponentTypeId == request.ComponentTypeId && 
                a.ComponentTypeAttributeId == request.ComponentTypeAttributeId);
        
        if (existingAssoc != null)
            throw new BusinessException($"Association between ComponentType {request.ComponentTypeId} and Attribute {request.ComponentTypeAttributeId} already exists.");
        
        var newAssoc = new Data.Entities.ComponentTypeAttributeAssoc
        {
            ComponentTypeId = request.ComponentTypeId,
            ComponentTypeAttributeId = request.ComponentTypeAttributeId,
            IsActive = request.IsActive
        };
        
        await uow.Repository<Data.Entities.ComponentTypeAttributeAssoc>()
            .AddAsync(newAssoc);
        await uow.SaveChangesAsync();
        
        return new Response { Success = true };
    }
} 