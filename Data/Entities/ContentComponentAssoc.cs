namespace Data.Entities;

public class ContentComponentAssoc : Entity
{
    public Content Content { get; set; }
    public int ContentId { get; set; }

    public Component Component { get; set; }
    public int ComponentId { get; set; }
    
    public int Order { get; set; }
    public bool IsStatic { get; set; }
}