using Data.Entities;

namespace Application.Services.ComponentTypeAttributeAssoc.DTOs;

public class ComponentTypeAttributeAssocDto
{
    public int? Id { get; set; }
    public int ComponentTypeId { get; set; }
    public int ComponentTypeAttributeId { get; set; }
    public bool IsActive { get; set; } = true;
}