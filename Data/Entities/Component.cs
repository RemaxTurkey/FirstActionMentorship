namespace Data.Entities;

public class Component : Entity
{
    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }
    
    public bool IsActive { get; set; }
}