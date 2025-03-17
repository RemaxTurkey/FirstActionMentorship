using Application.Attributes;
using Application.Services.Base;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content;

public class GetContentCached : BaseSvc<GetContentCached.Request, GetContentCached.Response>
{
    public GetContentCached(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Cache("GetContentCached_{ContentId}", 300)]
    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var contentData = await GetContentDataInternalAsync(uow, req.ContentId);
        
        return new Response
        {
            ContentComponentAssoc = contentData.ContentComponentAssoc,
            Components = contentData.Components
        };
    }

    private async Task<(List<ContentComponentAssoc> ContentComponentAssoc, List<Data.Entities.Component> Components)>
        GetContentDataInternalAsync(GenericUoW uow, int? contentId)
    {
        var contentComponentAssoc = await uow.Repository<ContentComponentAssoc>()
            .FindByNoTracking(x => x.ContentId == contentId)
            .Include(x => x.Content.Parent)
            .Include(x => x.Component.ComponentType)
            .AsSplitQuery()
            .ToListAsync();

        var components = contentComponentAssoc
            .Select(x => x.Component)
            .ToList();

        return (contentComponentAssoc, components);
    }

    public record Request(int ContentId);

    public class Response
    {
        public List<ContentComponentAssoc> ContentComponentAssoc { get; set; }
        public List<Data.Entities.Component> Components { get; set; }
    }
} 