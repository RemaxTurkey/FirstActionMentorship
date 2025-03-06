using System.Linq;
using Application.Services.Component.DTOs;

namespace Application.Services.Component.Extensions;

public static partial class MappingExtensions
{
    public static ComponentDto ToDto(this Data.Entities.Component component) =>
        new()
        {
            Id = component.Id,
            ComponentTypeId = component.ComponentTypeId,
            IsActive = component.IsActive,
            ComponentItems = component.ComponentItems?
                .Select(item => new ComponentItemDto
                {
                    AttributeId = item.ComponentTypeAttributeId,
                    Value = item.Value
                })
                .ToList(),
            ComponentAttributes = component.ComponentAttributes?
                .Select(attr => new ComponentAttributeDto
                {
                    Id = attr.Id,
                    ComponentTypeAttributeId = attr.ComponentTypeAttributeId,
                    AttributeName = attr.ComponentTypeAttribute?.Name,
                    Value = attr.Value
                })
                .ToList()
        };
        
    public static ComponentAttributeDto ToDto(this Data.Entities.ComponentAttribute attribute) =>
        new()
        {
            Id = attribute.Id,
            ComponentTypeAttributeId = attribute.ComponentTypeAttributeId,
            AttributeName = attribute.ComponentTypeAttribute?.Name,
            Value = attribute.Value
        };
} 