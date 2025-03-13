namespace Data.Entities;

public class ContentCategoryEmployeeRecord : Entity
{
    public ContentCategoryEmployeeAssoc ContentCategoryEmployeeAssoc { get; set; }
    public int ContentCategoryEmployeeAssocId { get; set; }

    public Content Content { get; set; }
    public int ContentId { get; set; }
    
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedDate { get; set; }
}