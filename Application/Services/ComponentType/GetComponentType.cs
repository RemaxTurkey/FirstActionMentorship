using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Base;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.UnitOfWorks;

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
            throw new Exception("Component type not found");
            
        return new Response(Data: componentType.ToDto());
    }
} 