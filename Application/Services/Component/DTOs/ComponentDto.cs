using System.Collections.Generic;
using Application.Services.ComponentType.DTOs;

namespace Application.Services.Component.DTOs;

public class ComponentDto
{
    public int? Id { get; set; }
    public int ComponentTypeId { get; set; }
    
    public int? Order { get; set; }
    public List<ComponentItemDto> Items { get; set; }
    public List<ComponentTypeAttributeValueDto> ComponentTypeAttributeValues { get; set; }
    public bool IsActive { get; set; } = true;
    public ComponentTypeDto Type { get; set; }
    public int? ContentCategoryId { get; set; }
}

public class ComponentItemDto
{
    public string Value { get; set; }
    public int AttributeId { get; set; }
} 