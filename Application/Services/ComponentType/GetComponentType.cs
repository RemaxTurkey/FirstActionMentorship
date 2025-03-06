using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ComponentType;

public class GetComponentType(IServiceProvider serviceProvider)
    : BaseSvc<GetComponentType.Request, GetComponentType.Response>(serviceProvider)
    {
        public record Request(int Id);

        public record Response(ComponentTypeDto Data);

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var componentType = await uow.Repository<Data.Entities.ComponentType>()
            .GetByIdAsync(req.Id);

        if (componentType == null)
            throw new BusinessException("Component type not found");
        
        // ComponentType'ın DTO'sunu oluştur
        var componentTypeDto = componentType.ToDto();
        
        // Attribute'ları getir
        var attributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(uow, 
            new GetComponentTypeAttributes.Request { ComponentTypeId = req.Id });
        
        // Attribute'ları DTO'ya ekle
        componentTypeDto.Attributes = attributesResponse.Attributes;
            
        return new Response(Data: componentTypeDto);
    }
} 