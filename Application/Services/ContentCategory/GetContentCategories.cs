using Application.Models;
using Application.Services.Base;
using Application.Services.ContentCategory.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ContentCategory;

public class GetContentCategories(IServiceProvider serviceProvider)
    : BaseSvc<GetContentCategories.Request, GetContentCategories.Response>(serviceProvider)
{
    public record Request(PaginationModel Pagination = null);

    public record Response(
        List<ContentCategoryDto> Items,
        int? TotalCount = null,
        int? CurrentPage = null,
        int? TotalPages = null);

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        if (req.Pagination == null)
        {
            var allCategories = await uow.Repository<Data.Entities.ContentCategory>()
                .GetAll()
                .OrderBy(x => x.Order)
                .ToListAsync();

            return new Response(allCategories.Select(x => x.ToDto()).ToList());
        }

        var pagedResult = await uow.Repository<Data.Entities.ContentCategory>()
            .GetAllPagedAsync(req.Pagination);
        
        return new Response(
            Items: pagedResult.Items.Select(x => x.ToDto()).ToList(),
            TotalCount: pagedResult.TotalCount,
            CurrentPage: pagedResult.CurrentPage,
            TotalPages: pagedResult.TotalPages
        );
    }
}