using System.Text.Json.Serialization;

namespace Application.Models;

public class PaginationModel
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? OrderBy { get; set; } = "Id";
    public bool IsAscending { get; set; } = true;

    [JsonIgnore]
    public int Skip => (PageNumber - 1) * PageSize;
}

public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string OrderBy { get; set; }
    public bool IsAscending { get; set; }
    public List<T> Items { get; set; } = new();

    public PagedResult(List<T> items, int totalCount, int pageSize, int currentPage, string orderBy, bool isAscending)
    {
        Items = items;
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        OrderBy = orderBy;
        IsAscending = isAscending;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
} 