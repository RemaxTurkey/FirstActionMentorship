namespace Data.Entities;

public class ComponentItem : Entity
{
    public Component Component { get; set; }
    public int ComponentId { get; set; }

    public string Value { get; set; }
    public bool IsActive { get; set; }
}