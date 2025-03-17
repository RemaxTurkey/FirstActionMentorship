using Application.Attributes;
using Application.Services.Base;
using Application.Services.Component;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content;

public class GetComponentAttributesCached : BaseSvc<GetComponentAttributesCached.Request, GetComponentAttributesCached.Response>
{
    public GetComponentAttributesCached(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Cache("GetComponentAttributesCached_{ComponentTypeId}", 600)]
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var componentIds = req.ComponentIds;
        var componentTypeId = req.ComponentTypeId;

        var defaultComponentAttributes = (await Svc<GetComponentTypeAttributes>()
                .InvokeNoTrackingAsync(new GetComponentTypeAttributes.Request(componentTypeId)))
            .Attributes;

        var componentAttributeValues = await uow.Repository<ComponentAttributeValue>()
            .FindByNoTracking(x => componentIds.Contains(x.ComponentId))
            .Include(x => x.ComponentTypeAttribute)
            .ToListAsync();

        var attributesByComponentId = componentAttributeValues
            .GroupBy(x => x.ComponentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(a => a.ComponentTypeAttributeId, a => a)
            );

        return new Response
        {
            DefaultAttributes = defaultComponentAttributes,
            AttributesByComponentId = attributesByComponentId
        };
    }

    public record Request(int ComponentTypeId, List<int> ComponentIds);

    public class Response
    {
        public List<ComponentTypeAttributeDto> DefaultAttributes { get; set; }
        public Dictionary<int, Dictionary<int, ComponentAttributeValue>> AttributesByComponentId { get; set; }
    }
} 