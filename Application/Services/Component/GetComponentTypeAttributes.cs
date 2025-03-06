using Application.Exceptions;
using Application.Services.Base;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.Services.ComponentTypeAttribute.Extensions;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Component;

public class GetComponentTypeAttributes(IServiceProvider serviceProvider)
    : BaseSvc<GetComponentTypeAttributes.Request, GetComponentTypeAttributes.Response>(serviceProvider)
{
    public class Request
    {
        public int ComponentTypeId { get; set; }
    }
    
    public class Response
    {
        public List<ComponentTypeAttributeDto> Attributes { get; set; }
    }
    
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {
        var componentType = await uow.Repository<Data.Entities.ComponentType>()
            .GetByIdAsync(request.ComponentTypeId);
        
        if (componentType == null)
            throw new BusinessException($"ComponentType with ID {request.ComponentTypeId} not found.");
        
        var attributeAssocs = await uow.Repository<Data.Entities.ComponentTypeAttributeAssoc>()
            .FindByNoTracking(a => a.ComponentTypeId == request.ComponentTypeId)
            .ToListAsync();
        
        if (!attributeAssocs.Any())
            return new Response { Attributes = new List<ComponentTypeAttributeDto>() };
        
        var attributeIds = attributeAssocs.Select(a => a.ComponentTypeAttributeId).ToList();
        
        var attributes = await uow.Repository<Data.Entities.ComponentTypeAttribute>()
            .FindByNoTracking(a => attributeIds.Contains(a.Id))
            .ToListAsync();
        
        return new Response 
        { 
            Attributes = attributes.Select(a => a.ToDto()).ToList() 
        };
    }
} 