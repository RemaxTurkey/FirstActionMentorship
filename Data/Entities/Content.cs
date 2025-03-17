using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Content : Entity
{
    public Content Parent { get; set; }
    public int? ParentId { get; set; }
    
    public string Header { get; set; }
    public DateTime CreatedDate { get; set; }
    public PageType PageType { get; set; }

    public ICollection<ContentComponentAssoc> ContentComponentAssocs { get; set; }
}

public enum PageType
{
    Content,
    Menu,
    Static
}