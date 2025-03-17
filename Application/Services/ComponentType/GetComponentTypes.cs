using Application.Models;
using Application.Services.Base;
using Application.Services.ComponentType.DTOs;
using Application.Services.ComponentType.Extensions;
using Application.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.ComponentType;

public class GetComponentTypes(IServiceProvider serviceProvider)
    : BaseSvc<GetComponentTypes.Request, GetComponentTypes.Response>(serviceProvider)
{
    public record Request(PaginationModel Pagination = null);

    public record Response(
        List<ComponentTypeDto> Items,
        int? TotalCount = null,
        int? CurrentPage = null,
        int? TotalPages = null);

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        if (req.Pagination == null)
        {
            var allComponentTypes = await uow.Repository<Data.Entities.ComponentType>()
                .GetAll()
                .Include(x => x.ComponentTypeAttributeAssocs)
                .ThenInclude(x => x.ComponentTypeAttribute)
                .OrderBy(x => x.Id)
                .ToListAsync();

            return new Response(allComponentTypes.Select(x => x.ToDto()).ToList());
        }

        var pagedResult = await uow.Repository<Data.Entities.ComponentType>()
            .GetAllPagedAsync(req.Pagination);
        
        return new Response(
            Items: pagedResult.Items.Select(x => x.ToDto()).ToList(),
            TotalCount: pagedResult.TotalCount,
            CurrentPage: pagedResult.CurrentPage,
            TotalPages: pagedResult.TotalPages
        );
    }
} 