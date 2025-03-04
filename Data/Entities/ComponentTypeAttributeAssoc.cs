namespace Data.Entities;

public class ComponentTypeAttributeAssoc
{
    public int Id { get; set; }
    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }

    public ComponentTypeAttribute ComponentTypeAttribute { get; set; }
    public int ComponentTypeAttributeId { get; set; }
}