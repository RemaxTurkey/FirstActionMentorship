namespace Data.Entities;

public class ContentEmployeeRecord : Entity
{
    public ContentEmployeeAssoc ContentEmployeeAssoc { get; set; }
    public int ContentEmployeeAssocId { get; set; }

    public Content Content { get; set; }
    public int ContentId { get; set; }
    
    public DateTime? CompletionDate { get; set; }
    public int EmployeeId { get; set; }
    public int? PropertyId { get; set; }
}