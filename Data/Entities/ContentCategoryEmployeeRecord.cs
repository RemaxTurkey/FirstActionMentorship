namespace Data.Entities;

public class ContentCategoryEmployeeRecord
{
    public int Id { get; set; }
    
    public ContentCategoryEmployeeAssoc ContentCategoryEmployeeAssoc { get; set; }
    public int ContentCategoryEmployeeAssocId { get; set; }

    public Content Content { get; set; }
    public int ContentId { get; set; }

    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CompleteDate { get; set; }
}