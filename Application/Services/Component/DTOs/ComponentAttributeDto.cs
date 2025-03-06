namespace Application.Services.Component.DTOs;

public class ComponentAttributeDto
{
    public int? Id { get; set; }
    public int ComponentTypeAttributeId { get; set; }
    public string AttributeName { get; set; }
    public string Value { get; set; }
} 