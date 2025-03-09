using System.Collections.Generic;

namespace Application.Services.Component.DTOs;

public class ComponentDto
{
    public int? Id { get; set; }
    public int ComponentTypeId { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public List<ComponentItemDto> ComponentItems { get; set; }
    public List<ComponentTypeAttributeValueDto> ComponentTypeAttributeValues { get; set; }
}

public class ComponentItemDto
{
    public string Value { get; set; }
    public int AttributeId { get; set; }
} 