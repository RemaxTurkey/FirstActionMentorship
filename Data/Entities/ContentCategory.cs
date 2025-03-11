namespace Data.Entities;

public class ContentCategory : Entity
{
    public string Title { get; set; }

    public ContentCategory Parent { get; set; }
    public int? ParentId { get; set; }

    public int Order { get; set; }
    public string Icon { get; set; }
}