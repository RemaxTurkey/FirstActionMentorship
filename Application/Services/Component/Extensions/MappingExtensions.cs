using System.Linq;
using Application.Services.Component.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.Services.ComponentTypeAttribute.Extensions;

namespace Application.Services.Component.Extensions;

public static partial class MappingExtensions
{
    public static ComponentDto ToDto(this Data.Entities.Component component, int? order = null) =>
        new()
        {
            Id = component.Id,
            ComponentTypeId = component.ComponentTypeId,
            Type = component.ComponentType?.ToDto(),
            IsActive = component.IsActive,
            Order = order,
            ComponentTypeAttributeValues = component.ComponentAttributeValue?
                .Select(attr => new ComponentTypeAttributeValueDto
                {
                    Id = attr.Id,
                    ComponentTypeAttributeId = attr.ComponentTypeAttributeId,
                    ComponentTypeAttributeName = attr.ComponentTypeAttribute?.Name,
                    Value = attr.Value
                })
                .ToList()
        };
        
    public static ComponentTypeAttributeValueDto ToDto(this Data.Entities.ComponentAttributeValue attribute) =>
        new()
        {
            Id = attribute.Id,
            ComponentTypeAttributeId = attribute.ComponentTypeAttributeId,
            ComponentTypeAttributeName = attribute.ComponentTypeAttribute?.Name,
            Value = attribute.Value
        };
} 