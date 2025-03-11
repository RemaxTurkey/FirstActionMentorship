using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Content : Entity
{
    public string Title { get; set; }

    public ContentCategory ContentCategory { get; set; }
    public int ContentCategoryId { get; set; }
    
    
    public string Header { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<ContentComponentAssoc> ContentComponentAssocs { get; set; }
}