namespace Data.Entities;

public class ComponentTypeAttributeAssoc : Entity
{
    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }

    public ComponentTypeAttribute ComponentTypeAttribute { get; set; }
    public int ComponentTypeAttributeId { get; set; }
}