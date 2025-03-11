using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component.DTOs;
using Application.Services.Component.Extensions;
using Application.Services.Content.DTOs;
using Application.Services.ContentCategory.Extensions;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Content;

public class GetContent : BaseSvc<GetContent.Request, GetContent.Response>
{
    public record Request(int? Id);

    public class Response
    {
        // type içindeki description developerDescription
        // componentin içinden componentTypeId ve isActive çıkarabiliriz
        public ContentDto Item { get; set; }
    }

    public GetContent(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var contentComponentAssoc = await uow.Repository<ContentComponentAssoc>()
            .FindByNoTracking(x => x.ContentId == req.Id)
            .Include(x => x.Content.ContentCategory.Parent)
            
            .Include(x => x.Component.ComponentType)
            .AsSplitQuery()
            .ToListAsync();

        if (contentComponentAssoc.Count == 0)
        {
            throw new BusinessException("Content not found");
        }
        
        var contentDto = new ContentDto
        {
            IsActive = contentComponentAssoc.First().IsActive,
            ContentCategory = contentComponentAssoc.First().Content.ContentCategory.ToDto(),
            Id = contentComponentAssoc.First().ContentId,
            ContentCategoryId = contentComponentAssoc.First().Content.ContentCategoryId,
            Header = contentComponentAssoc.First().Content.Header,
            Title = contentComponentAssoc.First().Content.Title,
            CreatedDate = contentComponentAssoc.First().Content.CreatedDate,
        };
        
        var components = contentComponentAssoc
            .Select(x => x.Component)
            .ToList();
        
        var componentIds = components.Select(c => c.Id).ToList();
        
        Console.WriteLine("Component ID'leri: " + string.Join(", ", componentIds));
        
        var componentAttributeValues = await uow.Repository<ComponentAttributeValue>()
            .FindByNoTracking(x => componentIds.Contains(x.ComponentId))
            .Include(x => x.ComponentTypeAttribute)
            .ToListAsync();
            
        Console.WriteLine("Bulunan ComponentAttributeValue kayıt sayısı: " + componentAttributeValues.Count);
        
        if (componentAttributeValues.Count == 0 && componentIds.Any())
        {
            Console.WriteLine("Ana sorgu başarısız oldu, alternatif yöntemi deniyorum...");
            
            foreach (var componentId in componentIds)
            {
                var values = await uow.Repository<ComponentAttributeValue>()
                    .FindByNoTracking(x => x.ComponentId == componentId)
                    .Include(x => x.ComponentTypeAttribute)
                    .ToListAsync();
                
                Console.WriteLine($"Component ID {componentId} için bulunan kayıt sayısı: {values.Count}");
                componentAttributeValues.AddRange(values);
            }
        }
        
        var contentComponents = new List<ComponentDto>();
        
        foreach (var component in components)
        {
            var componentDto = component.ToDto();
            
            var componentAttrs = componentAttributeValues
                .Where(x => x.ComponentId == component.Id)
                .ToList();
                
            componentDto.ComponentTypeAttributeValues = componentAttrs
                .Select(attr => new ComponentTypeAttributeValueDto
                {
                    Id = attr.Id,
                    ComponentTypeAttributeId = attr.ComponentTypeAttributeId,
                    ComponentTypeAttributeName = attr.ComponentTypeAttribute?.Name,
                    Value = attr.Value
                })
                .ToList();
                
            componentDto.Order = contentComponentAssoc
                .FirstOrDefault(x => x.ComponentId == component.Id)?.Order ?? 0;
                
            contentComponents.Add(componentDto);
        }
        
        contentDto.Components = contentComponents;
        contentDto.Header = contentDto.Header ?? "";
        contentDto.Title = contentDto.Title ?? "";
        
        return new Response { Item = contentDto };
    }

    public class ContentDetailViewModel
    {
        public int ContentId { get; set; }
        public string Header { get; set; }
        public string ContentCategoryName { get; set; }
        public int ContentCategoryId { get; set; }

        public List<ComponentDetailItemViewModel> Items { get; set; }
    }

    public class ComponentDetailItemViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        
        /// <summary>
        /// Bu field dinamik olarak değişecek. Client burayı obje listesi olarak alacak. 
        /// Sahip olduğu her attribute için,
        /// Attributes:
        /// [
        ///     {attribute[0].Name:attribute[0].Value},
        ///     {attribute[1].Name:attribute[1].Value}
        ///     ...
        /// ] 
        /// şeklinde respose dönmeli.
        /// </summary>
        public object Attributes { get; set; }

    }
}