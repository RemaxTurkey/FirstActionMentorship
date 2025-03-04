namespace Data.Entities;

public class ContentComponentAssoc
{
    public int Id { get; set; }
    
    public Content Content { get; set; }
    public int ContentId { get; set; }

    public Component Component { get; set; }
    public int ComponentId { get; set; }
    
    public int Order { get; set; }
    public bool IsActive { get; set; }
}