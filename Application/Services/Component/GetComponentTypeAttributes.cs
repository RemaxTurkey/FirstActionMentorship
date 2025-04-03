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
using Application.Attributes;

namespace Application.Services.Component;

public class GetComponentTypeAttributes(IServiceProvider serviceProvider)
    : BaseSvc<GetComponentTypeAttributes.Request, GetComponentTypeAttributes.Response>(serviceProvider)
{
    public record Request(int ComponentTypeId);
    
    public class Response
    {
        public List<ComponentTypeAttributeDto> Attributes { get; set; }
    }
    
    // [Cache("ComponentTypeAttributes_{ComponentTypeId}")]
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request request)
    {        
        var attributeAssoc = await uow.Repository<Data.Entities.ComponentTypeAttributeAssoc>()
            .FindByNoTracking(a => a.ComponentTypeId == request.ComponentTypeId)
            .ToListAsync();
        
        if (!attributeAssoc.Any())
            return new Response { Attributes = new List<ComponentTypeAttributeDto>() };
        
        var attributeIds = attributeAssoc.Select(a => a.ComponentTypeAttributeId).ToList();
        
        var attributes = await uow.Repository<Data.Entities.ComponentTypeAttribute>()
            .FindByNoTracking(a => attributeIds.Contains(a.Id))
            .ToListAsync();
        
        return new Response 
        { 
            Attributes = attributes.Select(a => a.ToDto()).ToList() 
        };
    }
} 