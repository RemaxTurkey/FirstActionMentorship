using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Content
{
    public int Id { get; set; }
    public string Title { get; set; }

    public ContentCategory ContentCategory { get; set; }
    public int ContentCategoryId { get; set; }
    
    public bool IsActive { get; set; }
    public string Header { get; set; }
    public DateTime CreatedDate { get; set; }
}