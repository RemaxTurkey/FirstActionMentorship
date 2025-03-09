namespace Application.Services.Component.DTOs;

public class ComponentTypeAttributeValueDto
{
    public int? Id { get; set; }
    public int ComponentId { get; set; }
    public int ComponentTypeAttributeId { get; set; }
    public string ComponentTypeAttributeName { get; set; }
    public string Value { get; set; }
}