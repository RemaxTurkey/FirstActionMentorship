namespace Data.Entities;

public class ComponentAttributeValue : Entity
{
    public Component Component { get; set; }
    public int ComponentId { get; set; }
    
    public ComponentTypeAttribute ComponentTypeAttribute { get; set; }
    public int ComponentTypeAttributeId { get; set; }
    
    public string Value { get; set; }
} 