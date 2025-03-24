namespace Data.Entities;

public class ComponentItem : Entity
{
    public Component Component { get; set; }
    public int ComponentId { get; set; }
    public string Title { get; set; }
    public string RedirectUrl { get; set; }
    public string Icon { get; set; }
}