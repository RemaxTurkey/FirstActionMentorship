namespace Data.Entities;

public class ComponentTypeAttribute : Entity
{
    public string Name { get; set; }
    public string Value { get; set; }
    
    public ICollection<ComponentTypeAttributeAssoc> ComponentTypeAttributeAssocs { get; set; }
}