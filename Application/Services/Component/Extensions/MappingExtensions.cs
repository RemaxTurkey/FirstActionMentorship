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
                    AttributeId = item.Id,
                    Value = item.Value
                })
                .ToList()
        };
} 