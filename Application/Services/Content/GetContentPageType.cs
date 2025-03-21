using System.Threading.Tasks;
using Application.Attributes;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Application.Services.Content;

public class GetContentPageType : BaseSvc<GetContentPageType.Request, GetContentPageType.Response>
{
    public GetContentPageType(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Cache("GetContentPageType_{ContentId}", 3600)]
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var pageType = await uow.Repository<Data.Entities.Content>()
            .FindByNoTracking(x => x.Id == req.ContentId)
            .Select(x => x.PageType)
            .FirstAsync();

        return new Response(pageType);
    }

    public record Request(int ContentId);
    public record Response(PageType PageType);
} 