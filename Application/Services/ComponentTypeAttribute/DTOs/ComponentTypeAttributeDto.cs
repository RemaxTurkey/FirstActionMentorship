using Data.Entities;

namespace Application.Services.ComponentTypeAttribute.DTOs;

public class ComponentTypeAttributeDto
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public AttributeDataType DataType { get; set; } = AttributeDataType.String;
} 