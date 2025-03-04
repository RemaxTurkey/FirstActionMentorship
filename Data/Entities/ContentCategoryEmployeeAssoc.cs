namespace Data.Entities;

public class ContentCategoryEmployeeAssoc
{
    public int Id { get; set; }

    public ContentCategory ContentCategory { get; set; }
    public int ContentCategoryId { get; set; }
    
    public int EmployeeId { get; set; }

    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
}