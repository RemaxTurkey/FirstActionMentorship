namespace Data.Entities;

public class ComponentType : Entity
{
    public string Title { get; set; }

    public ICollection<ComponentTypeAttributeAssoc> ComponentTypeAttributeAssocs { get; set; }
}