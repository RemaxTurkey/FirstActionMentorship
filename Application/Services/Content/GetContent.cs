using System.Dynamic;
using System.Globalization;
using System.Threading.Tasks;
using Application.Constants;
using Application.Exceptions;
using Application.Services.Base;
using Application.Services.Component;
using Application.Services.ComponentTypeAttribute.DTOs;
using Application.Services.Employee;
using Application.UnitOfWorks;
using Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Content;

public class GetContent : BaseSvc<GetContent.Request, GetContent.Response>
{
    public GetContent(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private bool HazirlikCheckMarkStatus = false;

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

            foreach (var attr in componentTypeAttributes)
            {
                var attributeName = attr.Name;
                string stringValue = null;
                
                if (attributeName == Constants.Constants.CheckmarkAttributeName)
                {
                    stringValue = await GetCheckmarkAttributeValue(uow, component, req.EmployeeId);
                    if (component.Id == Constants.Constants.HazirlikComponentId)
                    {
                        HazirlikCheckMarkStatus = stringValue == "true";
                    }
                }
                else if (attributeName == Constants.Constants.LockStatusAttributeName)
                {
                    stringValue = await GetLockStatusAttributeValue(uow, component, req.EmployeeId, req.Id);
                }
                else if (attr.Id == Constants.Constants.ImageSliderAttributeId && component.ComponentTypeId == Constants.Constants.ImageSliderComponentTypeId)
                {
                    stringValue = component.ImageUrls;
                    
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        var imageUrls = stringValue.Split(',')
                            .Select(url => url.Trim())
                            .Where(url => !string.IsNullOrWhiteSpace(url))
                            .ToArray();
                            
                        componentExpando["imageUrls"] = imageUrls;
                        
                        continue;
                    }
                }
                else if (component.ComponentTypeId == Constants.Constants.ButtonGroupComponentTypeId)
                {
                    var componentItems = await uow.Repository<ButtonGroupDetail>()
                        .FindByNoTracking(x => x.ComponentId == component.Id)
                        .ToListAsync();

                    if (componentItems.Any())
                    {
                        var buttons = new List<IDictionary<string, object>>();
                        
                        foreach (var item in componentItems)
                        {
                            var buttonObj = new ExpandoObject() as IDictionary<string, object>;
                            
                            if (!string.IsNullOrEmpty(item.Icon))
                                buttonObj["icon"] = item.Icon;
                            
                            if (!string.IsNullOrEmpty(item.RedirectUrl))
                                buttonObj["redirectUrl"] = item.RedirectUrl;
                            
                            if (!string.IsNullOrEmpty(item.Title))
                                buttonObj["title"] = item.Title;
                            
                            if (buttonObj.Count > 0)
                                buttons.Add(buttonObj);
                        }
                        
                        componentExpando["buttons"] = buttons;
                        
                        continue;
                    }
                }
                else
                {
                    var attributeValue = componentAttributeValues
                        .FirstOrDefault(cav => cav.ComponentTypeAttributeId == attr.Id);

                    if (attributeValue != null)
                    {
                        stringValue = attributeValue.Value;
                    }
                }

                var typedValue = ConvertToTypedValue(stringValue, attr.DataType);
                componentExpando[attributeName] = typedValue;
            }

            dynamicComponents.Add(componentExpando);
        }

        return dynamicComponents;
    }

    // private async Task<string> GetPowerStartPercentage(GenericUoW uow, int employeeId)
    // {
    //     var sql = @$"SELECT 
    //                     eam.CompletionPercentage AS Value
    //                 FROM dbo.EmployeeAcademy ea
    //                 INNER JOIN dbo.EmployeeAcademyModule eam ON eam.EmployeeAcademyId = ea.Id 
    //                 WHERE ea.EmployeeId = {employeeId}";
    //
    //     var percentage = await uow.DbContext.Database
    //         .SqlQueryRaw<decimal>(sql)
    //         .FirstOrDefaultAsync();
    //
    //     return percentage >= 83 ? "true" : "false";
    // }

    private async Task<string> GetLockStatusAttributeValue(GenericUoW uow, Data.Entities.Component component,
        int employeeId, int? contentId)
    {
        if (contentId != 5)
        {
            return "false";
        }
        if (component.Id == Constants.Constants.HazirlikComponentId)
        {
            return "false";
        }
        if (!component.ContentId.HasValue)
        {
            return "false";
        }
        var contentIsHazirlik = await uow.Repository<Data.Entities.Content>()
            .FindByNoTracking(c => c.Id == component.ContentId.Value)
            .FirstOrDefaultAsync();

        // hazırlık id=2. content hazırlık altındaki bir menü ise kontrol gerektiği için kontrol ekledim.
        if (contentIsHazirlik == null && contentIsHazirlik.Id != Constants.Constants.ContentHazirlikId)
        {
            return "false";
        }

        if (!HazirlikCheckMarkStatus)
        {
            return "true";
        }

        // ContentEmployeeAssoc'ta bu employee için bu content'in hazırlık tamamlandı kaydı varsa lockStatus false, yoksa true dönmeli.
        var contentIsHazirlikEmployeeAssoc = await uow.Repository<ContentEmployeeAssoc>()
            .FindByNoTracking(c => c.ContentId == Constants.Constants.ContentHazirlikId 
                                   && c.EmployeeId == employeeId
                                   && c.IsCompleted)
            .AnyAsync();

        return contentIsHazirlikEmployeeAssoc ? "false" : "true";
    }
    
    private async Task<string> GetCheckmarkAttributeValue(GenericUoW uow, Data.Entities.Component component, int employeeId)
    {
        if (!component.ContentId.HasValue)
        {
            return "false";
        }

        var contentPageTypeResponse = await Svc<GetContentPageType>().InvokeAsync(
            uow, 
            new GetContentPageType.Request(component.ContentId.Value));

        var employeeAssocResponse = await Svc<CheckEmployeeContentAssociation>().InvokeAsync(
            uow,
            new CheckEmployeeContentAssociation.Request(
                component.ContentId.Value,
                employeeId,
                contentPageTypeResponse.PageType));

        return employeeAssocResponse.Exists ? "true" : "false";
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