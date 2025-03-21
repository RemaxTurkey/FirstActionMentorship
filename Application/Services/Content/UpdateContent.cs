using System.Diagnostics;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component;
using Application.Services.Component.DTOs;
using Application.Services.Content.DTOs;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Application.RedisCache;
using Microsoft.Extensions.Logging;

namespace Application.Services.Content;

public class UpdateContent : BaseSvc<UpdateContent.Request, UpdateContent.Response>
{
    public UpdateContent(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        ValidateComponents(req);

        var content = await uow.Repository<Data.Entities.Content>()
            .FindBy(c => c.Id == req.Id)
            .FirstOrDefaultAsync();

        if (content == null)
        {
            throw new BusinessException($"Content with ID {req.Id} not found");
        }

        var oldPageType = content.PageType;
        
        content.Header = req.Header;
        content.ParentId = req.ParentId;
        content.PageType = req.PageType;

        uow.Repository<Data.Entities.Content>().Update(content);
        await uow.SaveChangesAsync();

        await InvalidateContentCache(content.Id);

        var existingContentComponents = await uow.Repository<ContentComponentAssoc>()
            .FindBy(cca => cca.ContentId == content.Id && cca.IsActive)
            .Include(cca => cca.Component)
            .ToListAsync();

        foreach (var componentDto in req.Components)
        {
            ContentComponentAssoc contentComponentAssoc;
            Data.Entities.Component component;

            if (componentDto.Id.HasValue)
            {
                contentComponentAssoc = existingContentComponents
                    .FirstOrDefault(cca => cca.ComponentId == componentDto.Id.Value);

                if (contentComponentAssoc == null)
                {
                    throw new BusinessException($"Component with ID {componentDto.Id.Value} not found for this content");
                }

                component = contentComponentAssoc.Component;
                
                contentComponentAssoc.Order = componentDto.Order;
                uow.Repository<ContentComponentAssoc>().Update(contentComponentAssoc);
                
                component.ComponentTypeId = componentDto.ComponentTypeId;
                uow.Repository<Data.Entities.Component>().Update(component);
            }
            else
            {
                component = new Data.Entities.Component
                {
                    ComponentTypeId = componentDto.ComponentTypeId,
                    IsActive = true
                };

                contentComponentAssoc = new ContentComponentAssoc()
                {
                    Component = component,
                    IsActive = true,
                    ContentId = content.Id,
                    Order = componentDto.Order
                };

                await uow.Repository<ContentComponentAssoc>().AddAsync(contentComponentAssoc);
            }

            await uow.SaveChangesAsync();

            int componentId = component.Id;

            if (componentDto.Id.HasValue)
            {
                var existingAttributeValues = await uow.Repository<ComponentAttributeValue>()
                    .FindBy(ctav => ctav.ComponentId == componentId && ctav.IsActive)
                    .ToListAsync();

                foreach (var existingAttr in existingAttributeValues)
                {
                    var updatedAttr = componentDto.AttributeValue
                        .FirstOrDefault(a => a.Id.HasValue && a.Id.Value == existingAttr.Id);

                    if (updatedAttr != null)
                    {
                        existingAttr.Value = updatedAttr.Value;
                        uow.Repository<ComponentAttributeValue>().Update(existingAttr);
                    }
                    else
                    {
                        existingAttr.IsActive = false;
                        uow.Repository<ComponentAttributeValue>().Update(existingAttr);
                    }
                }
            }

            foreach (var attribute in componentDto.AttributeValue.Where(a => !a.Id.HasValue))
            {
                await Svc<AddComponentTypeAttributeValue>().InvokeAsync(uow, new AddComponentTypeAttributeValue.Request
                {
                    ComponentId = componentId,
                    AttributeValue = new ComponentTypeAttributeValueDto
                    {
                        ComponentId = componentId,
                        ComponentTypeAttributeId = attribute.ComponentTypeAttributeId,
                        Value = attribute.Value
                    }
                });
            }

            if (componentDto.Id.HasValue && componentDto.Items != null)
            {
                var existingItems = await uow.Repository<ComponentItem>()
                    .FindBy(ci => ci.ComponentId == componentId && ci.IsActive)
                    .ToListAsync();

                foreach (var existingItem in existingItems)
                {
                    var updatedItem = componentDto.Items
                        .FirstOrDefault(i => i.Id.HasValue && i.Id.Value == existingItem.Id);

                    if (updatedItem != null)
                    {
                        existingItem.Value = updatedItem.Value;
                        uow.Repository<ComponentItem>().Update(existingItem);
                    }
                    else
                    {
                        existingItem.IsActive = false;
                        uow.Repository<ComponentItem>().Update(existingItem);
                    }
                }
            }

            if (componentDto.Items != null)
            {
                foreach (var item in componentDto.Items.Where(i => !i.Id.HasValue))
                {
                    await Svc<AddComponentItem>().InvokeAsync(uow, new AddComponentItem.Request
                    {
                        ComponentId = componentId,
                        ComponentTypeId = componentDto.ComponentTypeId,
                        Item = item,
                        IsActive = true
                    });
                }
            }
        }

        var componentIdsInRequest = req.Components
            .Where(c => c.Id.HasValue)
            .Select(c => c.Id.Value)
            .ToList();

        var componentsToDelete = existingContentComponents
            .Where(cca => !componentIdsInRequest.Contains(cca.ComponentId))
            .ToList();

        foreach (var componentToDelete in componentsToDelete)
        {
            componentToDelete.IsActive = false;
            uow.Repository<ContentComponentAssoc>().Update(componentToDelete);

            componentToDelete.Component.IsActive = false;
            uow.Repository<Data.Entities.Component>().Update(componentToDelete.Component);
        }
        
        await uow.SaveChangesAsync();

        var updatedContentResponse = await Svc<GetContentForUpdate>().InvokeAsync(uow, new GetContentForUpdate.Request(content.Id));

        return new Response
        {
            Item = updatedContentResponse.Item
        };
    }

    private void ValidateComponents(Request request)
    {
        if (request.Components is null)
        {
            throw new BusinessException("Components are required");
        }
    }

    private async Task InvalidateContentCache(int contentId)
    {
        if (CacheManager != null)
        {
            var pageTypeCacheKey = $"GetContentPageType_{contentId}";
            await CacheManager.RemoveAsync(pageTypeCacheKey);
            
            var contentCachedKey = $"GetContentCached_{contentId}";
            await CacheManager.RemoveAsync(contentCachedKey);
            
            Logger?.Log(LogLevel.Debug, $"Content ID {contentId} için cache temizlendi");
        }
    }

    public class Request
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Header { get; set; }
        public List<UpdateContentComponentDto> Components { get; set; }
        public PageType PageType { get; set; }
    }
    
    public class UpdateContentComponentDto 
    {
        public int? Id { get; set; }
        public int ComponentTypeId { get; set; }
        public int Order { get; set; }
        public List<UpdateComponentItemDto> Items { get; set; }
        public List<UpdateContentComponentTypeAttributeValueDto> AttributeValue { get; set; } = new();
    }

    public class UpdateContentComponentTypeAttributeValueDto
    {
        public int? Id { get; set; }
        public int ComponentTypeAttributeId { get; set; }
        public string Value { get; set; }
    }

    public class UpdateComponentItemDto : ComponentItemDto
    {
        public int? Id { get; set; }
    }

    public class Response
    {
        public GetContentForUpdate.ContentDetailViewModel Item { get; set; }
    }
} 