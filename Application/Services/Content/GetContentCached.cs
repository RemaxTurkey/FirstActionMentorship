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
        // Temel ilişkileri getir
        var contentComponentAssoc = await uow.Repository<ContentComponentAssoc>()
            .FindByNoTracking(x => x.ContentId == contentId)
            .ToListAsync();

        if (contentComponentAssoc == null || !contentComponentAssoc.Any())
        {
            return (new List<ContentComponentAssoc>(), new List<Data.Entities.Component>());
        }

        // Component ve Content ID'lerini güvenli şekilde al
        var componentIds = contentComponentAssoc
            .Where(x => x.ComponentId > 0)
            .OrderBy(x => x.Order)
            .Select(x => x.ComponentId)
            .Distinct()
            .ToList();

        var contentIds = contentComponentAssoc
            .Select(x => x.ContentId)
            .Distinct()
            .ToList();

        // Bileşenleri getir
        var components = new List<Data.Entities.Component>();
        if (componentIds.Any())
        {
            components = await uow.Repository<Data.Entities.Component>()
                .FindByNoTracking(x => componentIds.Contains(x.Id))
                .ToListAsync();
        }

        // Bileşen tiplerini getir
        var componentTypeIds = components
            .Where(x => x.ComponentTypeId > 0)
            .Select(x => x.ComponentTypeId)
            .Distinct()
            .ToList();

        var componentTypes = new List<Data.Entities.ComponentType>();
        if (componentTypeIds.Any()) 
        {
            componentTypes = await uow.Repository<Data.Entities.ComponentType>()
                .FindByNoTracking(x => componentTypeIds.Contains(x.Id))
                .ToListAsync();
        }

        // İçerikleri getir
        var contents = new List<Data.Entities.Content>();
        if (contentIds.Any())
        {
            contents = await uow.Repository<Data.Entities.Content>()
                .FindByNoTracking(x => contentIds.Contains(x.Id))
                .ToListAsync();
        }

        // Parent içerikleri getir
        var parentIds = contents
            .Where(x => x.ParentId.HasValue && x.ParentId.Value > 0)
            .Select(x => x.ParentId.Value)
            .Distinct()
            .ToList();

        var parentContents = new List<Data.Entities.Content>();
        if (parentIds.Any())
        {
            parentContents = await uow.Repository<Data.Entities.Content>()
                .FindByNoTracking(x => parentIds.Contains(x.Id))
                .ToListAsync();
        }

        // İlişkileri manuel olarak eşleştir
        foreach (var component in components)
        {
            component.ComponentType = componentTypes.FirstOrDefault(ct => ct.Id == component.ComponentTypeId);
        }

        foreach (var content in contents)
        {
            if (content.ParentId.HasValue)
            {
                content.Parent = parentContents.FirstOrDefault(p => p.Id == content.ParentId);
            }
        }

        foreach (var assoc in contentComponentAssoc)
        {
            assoc.Component = components.FirstOrDefault(c => c.Id == assoc.ComponentId);
            assoc.Content = contents.FirstOrDefault(c => c.Id == assoc.ContentId);
        }

        var orderedComponents = contentComponentAssoc
            .OrderBy(x => x.Order)
            .Select(x => components.FirstOrDefault(c => c.Id == x.ComponentId))
            .Where(x => x != null)
            .Distinct()
            .ToList();

        return (contentComponentAssoc, orderedComponents);
    }

    public record Request(int ContentId);

    public class Response
    {
        public List<ContentComponentAssoc> ContentComponentAssoc { get; set; }
        public List<Data.Entities.Component> Components { get; set; }
    }
} 