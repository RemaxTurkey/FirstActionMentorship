using Application.Services.Component.DTOs;
using Application.Services.ContentCategory.DTOs;

namespace Application.Services.Content.DTOs;

public class ContentDto
{
    public string Title { get; set; }
    public ContentCategoryDto ContentCategory { get; set; }
    public int ContentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public string Header { get; set; }
    public DateTime CreatedDate { get; set; }

    public List<ComponentDto> Components { get; set; }
}