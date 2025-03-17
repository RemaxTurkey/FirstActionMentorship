using System.Dynamic;
using System.Globalization;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Content;

public class GetContent : BaseSvc<GetContent.Request, GetContent.Response>
{
    public GetContent(IServiceProvider serviceProvider) : base(serviceProvider)
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

        if (contentData.ContentComponentAssoc.Count == 0) 
        {
            throw new BusinessException("Content not found");
        }

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

        var isMenu = contentData.ContentComponentAssoc.First().Content.IsMenu;

        if (!isMenu)
        {
            var contentEmployeeAssoc = await uow.Repository<ContentEmployeeAssoc>()
                .FindBy(x => x.ContentId == contentData.ContentComponentAssoc.First().ContentId && x.EmployeeId == req.EmployeeId)
                .FirstOrDefaultAsync();

            if (contentEmployeeAssoc == null)
            {
                contentEmployeeAssoc = new ContentEmployeeAssoc
                {
                    ContentId = contentData.ContentComponentAssoc.First().Content.ParentId!.Value,
                    EmployeeId = req.EmployeeId,
                    CompletionDate = DateTime.Now,
                    IsActive = true
                };

                await uow.Repository<ContentEmployeeAssoc>().AddAsync(contentEmployeeAssoc);
            }
        }

        return new Response
        {
            Item = new ContentDetailViewModel
            {
                ContentId = contentData.ContentComponentAssoc.First().ContentId,
                ParentId = contentData.ContentComponentAssoc.First().Content.ParentId,
                Header = contentData.ContentComponentAssoc.First().Content.Header ?? string.Empty,
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
            {
                var attributeName = attr.Name;
                string stringValue = null;
                
                if (componentAttributes.TryGetValue(attr.Id!.Value, out var attrValue))
                {
                    stringValue = attrValue.Value;
                }
                
                var typedValue = ConvertToTypedValue(stringValue, attr.DataType);
                componentExpando[attributeName] = typedValue;
            }

            dynamicComponents.Add(componentExpando);
        }
        
        return dynamicComponents;
    }
    
    private object ConvertToTypedValue(string value, Data.Entities.AttributeDataType dataType)
    {
        if (string.IsNullOrEmpty(value))
        {
            if (dataType == AttributeDataType.Boolean)
            {
                Logger?.Log(LogLevel.Debug, "Boolean type with empty value, returning FALSE");
                return false;
            }
            
            var result = dataType switch
            {
                AttributeDataType.String => string.Empty,
                AttributeDataType.Integer => null,
                AttributeDataType.Decimal => null, 
                AttributeDataType.DateTime => null,
                _ => null
            };

            return result;
        }
            
        try
        {
            switch (dataType)
            {
                case AttributeDataType.Boolean:
                    return bool.TryParse(value, out var boolResult) && boolResult;

                case AttributeDataType.String:
                    return value;
                    
                case AttributeDataType.Integer:
                    return int.TryParse(value, out var intResult) ? intResult : null;
                    
                case AttributeDataType.Decimal:
                    return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalResult) ? decimalResult : null;
                    
                case AttributeDataType.DateTime:
                    return DateTime.TryParse(value, out var dateResult) ? dateResult : null;
                    
                default:
                    return value;
            }
        }
        catch (Exception ex)
        {
            Logger?.Log(LogLevel.Warning, $"Error converting value '{value}' to {dataType}: {ex.Message}");
            
            if (dataType == AttributeDataType.Boolean)
                return false;
                
            return dataType switch
            {
                AttributeDataType.String => string.Empty,
                AttributeDataType.Integer => null,
                AttributeDataType.Decimal => null,
                AttributeDataType.DateTime => null,
                _ => null
            };
        }
    }

    public record Request(int? Id, int EmployeeId);

    public class Response
    {
        public ContentDetailViewModel Item { get; set; }
    }

    public class ContentDetailViewModel
    {
        public int ContentId { get; set; }
        public string Header { get; set; }
        public string ParentName { get; set; }
        public int? ParentId { get; set; }

        public dynamic Components { get; set; }
    }

    public class ComponentDetailItemViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
    }
}