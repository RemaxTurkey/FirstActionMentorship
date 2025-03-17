using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.Services.ComponentTypeAttribute.Extensions;
using Application.Services.ComponentTypeAttributeAssoc;
using Application.Services.ComponentTypeAttributeAssoc.DTOs;

namespace Application.Services.ComponentType.Extensions;

public static class MappingExtension
{
    public static ComponentTypeDto ToDto(this Data.Entities.ComponentType componentType)
    {
        return new ComponentTypeDto
        {
            Id = componentType.Id,
            Title = componentType.Title,
            Description = componentType.Description,
            Attributes = componentType.ComponentTypeAttributeAssocs
                ?.Select(x => x.ComponentTypeAttribute?.ToDto())
                .ToList()
        };
    }

    public static ComponentTypeAttributeDto ToDto(this Data.Entities.ComponentTypeAttribute attribute)
    {
        return new ComponentTypeAttributeDto
        {
            Id = attribute.Id,
            Name = attribute.Name,
            DataType = attribute.DataType
        };
    }
}
