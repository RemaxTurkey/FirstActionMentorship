namespace Application.Services.ContentCategory.DTOs;

public class ContentCategoryDto
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public int? ParentId { get; set; }
    public int Order { get; set; }
    public bool? IsActive { get; set; }
    public string Icon { get; set; }
}