namespace Data.Entities;

public class Component : Entity
{
    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }

    public int? ContentId { get; set; }
    public ICollection<ComponentItem> ComponentItems { get; set; }
    public ICollection<ContentComponentAssoc> ContentComponentAssoc { get; set; }
    public ICollection<ComponentAttributeValue> ComponentAttributeValue { get; set; }
}