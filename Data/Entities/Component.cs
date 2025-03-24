namespace Data.Entities;

public class Component : Entity
{
    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }
    public string ImageUrls { get; set; }
    public int? ContentId { get; set; }
    public ICollection<ButtonGroupDetail> ButtonGroupDetail { get; set; }
    public ICollection<ContentComponentAssoc> ContentComponentAssoc { get; set; }
    public ICollection<ComponentAttributeValue> ComponentAttributeValue { get; set; }
}