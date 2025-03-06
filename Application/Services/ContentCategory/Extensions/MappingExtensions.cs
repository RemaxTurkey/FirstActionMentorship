using Application.Services.ContentCategory.DTOs;

namespace Application.Services.ContentCategory.Extensions;

public static partial class MappingExtensions
{
    public static ContentCategoryDto ToDto(this Data.Entities.ContentCategory category) =>
        new()
        {
            Id = category.Id,
            Title = category.Title,
            ParentId = category.ParentId,
            Order = category.Order,
            IsActive = category.IsActive,
            Icon = category.Icon
        };
}