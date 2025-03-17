namespace Data.Entities;

public class ContentEmployeeAssoc : Entity
{
    public Content Content { get; set; }
    public int ContentId { get; set; }
    
    public int EmployeeId { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedDate { get; set; }
}