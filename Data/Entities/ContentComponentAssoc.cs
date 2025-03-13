namespace Data.Entities;

public class ContentComponentAssoc : Entity
{
    public Content Content { get; set; }
    public int ContentId { get; set; }

    public Component Component { get; set; }
    public int ComponentId { get; set; }
    
    public int Order { get; set; }

    /// <summary>
    /// Burası bir content category elemanı için bir component ise dolu olacak.
    /// </summary>
    public int? ContentCategoryId { get; set; }
    
}