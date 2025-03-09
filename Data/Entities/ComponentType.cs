namespace Data.Entities;

public class ComponentType : Entity
{
    public string Title { get; set; }

    // TODO: Bu componentin ne olduğunu açıklamak için eklendi
    public string Description { get; set; }
    
    public ICollection<ComponentTypeAttributeAssoc> ComponentTypeAttributeAssocs { get; set; }
}