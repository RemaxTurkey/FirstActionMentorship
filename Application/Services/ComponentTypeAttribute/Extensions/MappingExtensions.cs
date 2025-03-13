using Application.Services.ComponentTypeAttribute.DTOs;

namespace Application.Services.ComponentTypeAttribute.Extensions;

public static partial class MappingExtensions
{
    public static ComponentTypeAttributeDto ToDto(this Data.Entities.ComponentTypeAttribute attribute) =>
        new()
        {
            Id = attribute.Id,
            Name = attribute.Name, 
            DataType = attribute.DataType 
        };
} 