namespace Data.Entities;

public class Component
{
    public int Id { get; set; }

    public ComponentType ComponentType { get; set; }
    public int ComponentTypeId { get; set; }
    
    public bool IsActive { get; set; }
}