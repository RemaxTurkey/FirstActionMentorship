using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.ComponentType.DTOs;

namespace Application.Services.ComponentType.Extensions;

public static class MappingExtension
{
    public static ComponentTypeDto ToDto(this Data.Entities.ComponentType componentType)
    {
        return new ComponentTypeDto
        {
            Id = componentType.Id,
            Title = componentType.Title
        };
    }
}
