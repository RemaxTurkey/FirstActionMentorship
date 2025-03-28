using System.Dynamic;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.UnitOfWorks;
using Data.Entities;

namespace Application.Services.Content;

public class GetContentForUpdate : BaseSvc<GetContentForUpdate.Request, GetContentForUpdate.Response>
{
    public GetContentForUpdate(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var cachedContentResponse = await Svc<GetContentCached>().InvokeAsync(
            uow,
            new GetContentCached.Request(req.Id.Value));

        var contentData = (
            cachedContentResponse.ContentComponentAssoc,
            cachedContentResponse.Components
        );

        if (contentData.ContentComponentAssoc.Count == 0) throw new BusinessException("Content not found");

        var componentIds = contentData.Components.Select(c => c.Id).ToList();
        var componentTypeId = contentData.Components.First().ComponentTypeId;

        var cachedAttributeResponse = await Svc<GetComponentAttributesCached>().InvokeAsync(
            uow,
            new GetComponentAttributesCached.Request(componentTypeId, componentIds));

        var attributeData = (
            cachedAttributeResponse.DefaultAttributes,
            cachedAttributeResponse.AttributesByComponentId
        );

        var dynamicComponents = CreateDynamicComponents(
            contentData.Components,
            contentData.ContentComponentAssoc,
            attributeData.DefaultAttributes,
            attributeData.AttributesByComponentId);

        return new Response
        {
            Item = new ContentDetailViewModel
            {
                ContentId = contentData.ContentComponentAssoc.First().ContentId,
                ParentId = contentData.ContentComponentAssoc.First().Content.ParentId,
                Header = contentData.ContentComponentAssoc.First().Content.Header ?? string.Empty,
                PageType = contentData.ContentComponentAssoc.First().Content.PageType,
                Components = dynamicComponents
            }
        };
    }

    private List<dynamic> CreateDynamicComponents(
        List<Data.Entities.Component> components,
        List<ContentComponentAssoc> contentComponentAssoc,
        List<ComponentTypeAttributeDto> defaultAttributes,
        Dictionary<int, Dictionary<int, ComponentAttributeValue>> attributesByComponentId)
    {
        var dynamicComponents = new List<dynamic>();
        var componentOrderMap = contentComponentAssoc.ToDictionary(x => x.ComponentId, x => x.Order);

        foreach (var component in components)
        {
            var componentAttributes = attributesByComponentId.TryGetValue(component.Id, out var value)
                ? value
                : new Dictionary<int, ComponentAttributeValue>();

            var componentExpando = new ExpandoObject() as IDictionary<string, object>;

            componentExpando["Id"] = component.Id;
            componentExpando["TypeId"] = component.ComponentType?.Id;
            componentExpando["Type"] = component.ComponentType?.Title;
            componentExpando["Order"] = componentOrderMap.GetValueOrDefault(component.Id, 0);

            foreach (var attr in defaultAttributes)
                if (componentAttributes.TryGetValue(attr.Id!.Value, out var attributeValue))
                    componentExpando[attr.Name] = attributeValue.Value;
                else
                    componentExpando[attr.Name] = string.Empty;

            dynamicComponents.Add(componentExpando);
        }

        return dynamicComponents;
    }

    public class Request
    {
        public Request(int id)
        {
            Id = id;
        }

        public int? Id { get; set; }
    }

    public class Response
    {
        public ContentDetailViewModel Item { get; set; }
    }

    public class ContentDetailViewModel
    {
        public int ContentId { get; set; }
        public int? ParentId { get; set; }
        public string Header { get; set; }
        public List<dynamic> Components { get; set; }
        public PageType PageType { get; set; }
    }
}