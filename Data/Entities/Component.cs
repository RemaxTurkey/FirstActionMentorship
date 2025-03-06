namespace Data.Entities;

public class Component : Entity
{
    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }
    
    public bool IsActive { get; set; }

    public ICollection<ComponentItem> ComponentItems { get; set; }
    public ICollection<ContentComponentAssoc> ContentComponentAssocs { get; set; }
    public ICollection<ComponentAttribute> ComponentAttributes { get; set; }
}