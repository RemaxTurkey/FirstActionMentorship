using System.Dynamic;
using System.Globalization;
using System.Threading.Tasks;
using Application.Constants;
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

        Data.Entities.Content content = null;
        if (cachedContentResponse.ContentComponentAssoc.Count == 0)
        {
            content = uow.Repository<Data.Entities.Content>().FindByNoTracking(x => x.Id == req.Id).First();
        }
        else
        {
            content = contentData.ContentComponentAssoc.First().Content;
        }

        // Her bir component için ayrı ayrı işlem yapacağımız için, burada genel ComponentTypeId kullanmıyoruz
        var dynamicComponents = await CreateDynamicComponents(
            uow,
            req,
            contentData.Components,
            contentData.ContentComponentAssoc,
            content.PageType);

        var isContent = content.PageType;

        if (isContent == PageType.Content)
        {
            await Svc<AssignContentToEmployee>().InvokeAsync(
                uow,
                new AssignContentToEmployee.Request(
                    content.Id,
                    content.ParentId!.Value,
                    req.EmployeeId));
        }

        // content page type menüyse
        // ContentComponentAssoc tablosunda bu component IsStatic işaretlendiyse
        // 

        return new Response
        {
            Item = new ContentDetailViewModel
            {
                ContentId = content.Id,
                ParentId = content.ParentId,
                Header = content.Header ?? string.Empty,
                PageType = content.PageType,
                Components = dynamicComponents
            }
        };
    }

    private async Task<List<dynamic>> CreateDynamicComponents(GenericUoW uow,
        Request req,
        List<Data.Entities.Component> components,
        List<ContentComponentAssoc> contentComponentAssoc, PageType contentPageType)
    {
        var dynamicComponents = new List<dynamic>();
        var componentOrderMap = contentComponentAssoc.ToDictionary(x => x.ComponentId, x => x.Order);

        // Her bir component için ayrı ayrı işlem yap
        foreach (var component in components)
        {
            // Her component için kendi ComponentTypeId'sini kullan
            var componentTypeId = component.ComponentTypeId;

            // Bu component tipi için attribute'ları al
            var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeAsync(
                uow, new GetComponentTypeAttributes.Request(componentTypeId));

            var componentTypeAttributes = typeAttributesResponse.Attributes;

            // Component'in mevcut attribute değerlerini al
            var componentAttributeValues = await uow.Repository<ComponentAttributeValue>()
                .FindByNoTracking(cav => cav.ComponentId == component.Id && cav.IsActive)
                .Include(cav => cav.ComponentTypeAttribute)
                .ToListAsync();

            // Component için dynamicObject oluştur
            var componentExpando = new ExpandoObject() as IDictionary<string, object>;

            componentExpando["Id"] = component.Id;
            componentExpando["TypeId"] = component.ComponentType?.Id;
            componentExpando["Type"] = component.ComponentType?.Title;
            componentExpando["Order"] = componentOrderMap.GetValueOrDefault(component.Id, 0);

            // Her bir attribute için değeri ata
            foreach (var attr in componentTypeAttributes)
            {
                var attributeName = attr.Name;
                string stringValue = null;

                if (attributeName == Constants.Constants.CheckmarkAttributeName)
                {
                    if (!component.ContentId.HasValue)
                    {
                        stringValue = "false";
                    }
                    else
                    {
                        var componentinContentininPageType = await uow.Repository<Data.Entities.Content>()
                            .FindByNoTracking(x => x.Id == component.ContentId)
                            .Select(x => x.PageType)
                            .FirstAsync();

                        var exist = componentinContentininPageType == PageType.Content
                            ? await uow.Repository<ContentEmployeeRecord>()
                                .FindByNoTracking(x =>
                                    x.ContentId == component.ContentId && x.EmployeeId == req.EmployeeId)
                                .AnyAsync()
                            : componentinContentininPageType == PageType.Menu && await uow
                                .Repository<ContentEmployeeAssoc>()
                                .FindByNoTracking(x =>
                                    x.ContentId == component.ContentId && x.EmployeeId == req.EmployeeId)
                                .AnyAsync()
                            ;
                        stringValue = exist ? "true" : "false";
                    }
                }
                else
                {
                    // ComponentAttributeValue tablosundan doğrudan bu component ve attribute için değeri al
                    var attributeValue = componentAttributeValues
                        .FirstOrDefault(cav => cav.ComponentTypeAttributeId == attr.Id);

                    if (attributeValue != null)
                    {
                        stringValue = attributeValue.Value;
                    }
                }

                var typedValue = ConvertToTypedValue(stringValue, attr.DataType);

                // // Menü tipi kontrolleri
                // if (contentComponentAssoc.First().Content.PageType == PageType.Menu && 
                //     attributeName == "checkmarkStatus")
                // {
                //     // Componentin bağlı olduğu content'i bul
                //     var componentContentAssoc = contentComponentAssoc
                //         .FirstOrDefault(c => c.ComponentId == component.Id);
                //     
                //     if (componentContentAssoc != null)
                //     {
                //         // Bu content için employee kaydı var mı kontrol et
                //         var employeeRecord = await uow.Repository<ContentEmployeeAssoc>()
                //             .FindByNoTracking(c => 
                //                 c.ContentId == componentContentAssoc.ContentId && 
                //                 c.EmployeeId == req.EmployeeId)
                //             .FirstOrDefaultAsync();
                //         
                //         // Kayıt varsa true olarak işaretle
                //         if (employeeRecord != null)
                //         {
                //             typedValue = true;
                //         }
                //     }
                // }

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
                    return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var decimalResult)
                        ? decimalResult
                        : null;

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
        public PageType PageType { get; set; }
    }

    public class ComponentDetailItemViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
    }
}