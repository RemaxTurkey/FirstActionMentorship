using API.Controllers.Base;
using Application.Services.Component;
using Application.Services.Content;
using Application.Services.Content.DTOs;
using Application.Services.Employee;
using Application.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers;

public class ContentController : ApiControllerBase
{
    [HttpPost]
    public async Task<CreateContent.Response> CreateComponentType([FromBody] CreateContent.Request request)
    {
        var response = await Svc<CreateContent>().InvokeAsync(request);
        return response;
    }
    
    [HttpGet("{Id}/Employee/{EmployeeId}")]
    public async Task<GetContent.Response> GetContent([FromRoute] GetContent.Request request)
    {
        var response = await Svc<GetContent>().InvokeNoTrackingAsync(request);
        return response;
    }

    [HttpGet("{Id}/Employee/{EmployeeId}/Property/{PropertyId}")]
    public async Task<GetContent.Response> GetContentProperty([FromRoute] GetContent.Request request)
    {
        var response = await Svc<GetContent>().InvokeNoTrackingAsync(request);
        return response;
    }
    
    [HttpPost("save/{SaveContentId}/next/{NextContentId}/employee/{EmployeeId}")]
    public async Task<SaveContent.Response> SaveAndGetNextContent([FromRoute] SaveContent.Request request)
    {
        var response = await Svc<SaveContent>().InvokeAsync(request);
        return response;
    }

    [HttpPost("save/{SaveContentId}/next/{NextContentId}/employee/{EmployeeId}/property/{PropertyId}")]
    public async Task<SaveContent.Response> SavePropertyContent([FromRoute] SaveContent.Request request)
    {
        var response = await Svc<SaveContent>().InvokeAsync(request);
        return response;
    }

    [HttpPut("{Id}")]
    public async Task<UpdateContent.Response> UpdateContent([FromRoute] int Id, [FromBody] UpdateContent.Request request)
    {
        request.Id = Id;
        var response = await Svc<UpdateContent>().InvokeAsync(request);
        return response;
    }

    [HttpGet("edit/{Id}")]
    public async Task<UpdateContent.Request> GetContentForUpdate([FromRoute] int Id)
    {
        var request = new GetContentForUpdate.Request(Id);
        var contentResponse = await Svc<GetContentForUpdate>().InvokeNoTrackingAsync(request);
        
        // GetContentForUpdate.Response'u UpdateContent.Request'e dönüştürüyoruz
        var updateRequest = new UpdateContent.Request
        {
            Id = contentResponse.Item.ContentId,
            Header = contentResponse.Item.Header,
            ParentId = contentResponse.Item.ParentId,
            PageType = Data.Entities.PageType.Content, // Varsayılan olarak Content tipini kullanıyoruz, gerekirse değiştirilebilir
            Components = new List<UpdateContent.UpdateContentComponentDto>()
        };

        // Componentleri dönüştürme
        if (contentResponse.Item.Components != null)
        {
            foreach (dynamic component in contentResponse.Item.Components)
            {
                // Component dinamik olduğu için, gerekli özellikleri doğru şekilde elde etmemiz gerekiyor
                int componentId = (int)component.Id;
                int componentTypeId = (int)component.TypeId;
                int order = (int)component.Order;

                // ComponentType için geçerli attribute'ları al
                var typeAttributesResponse = await Svc<GetComponentTypeAttributes>().InvokeNoTrackingAsync(
                    new GetComponentTypeAttributes.Request(componentTypeId));
                
                var componentTypeAttributes = typeAttributesResponse.Attributes;
                
                var componentDto = new UpdateContent.UpdateContentComponentDto
                {
                    Id = componentId,
                    ComponentTypeId = componentTypeId,
                    Order = order,
                    AttributeValue = new List<UpdateContent.UpdateContentComponentTypeAttributeValueDto>()
                };

                // Component'in mevcut attribute değerlerini al
                var componentDetailResponse = await Svc<GetComponent>().InvokeNoTrackingAsync(
                    new GetComponent.Request { Id = componentId });
                
                var componentDetail = componentDetailResponse.Item;
                
                // Mevcut AttributeValue'ları ekle
                if (componentDetail.ComponentTypeAttributeValues != null)
                {
                    foreach (var attrValue in componentDetail.ComponentTypeAttributeValues)
                    {
                        componentDto.AttributeValue.Add(new UpdateContent.UpdateContentComponentTypeAttributeValueDto
                        {
                            Id = attrValue.Id,
                            ComponentTypeAttributeId = attrValue.ComponentTypeAttributeId,
                            Value = attrValue.Value
                        });
                    }
                }

                updateRequest.Components.Add(componentDto);
            }
        }

        return updateRequest;
    }

    [HttpPost("employee/video")]
    public async Task<SetEmployeeVideoLink.Response> SetEmployeeVideoLink([FromBody] SetEmployeeVideoLink.Request request)
    {
        var response = await Svc<SetEmployeeVideoLink>().InvokeAsync(request);
        return response;
    }

    [HttpGet("employee/video/{EmployeeId}")]
    public async Task<GetEmployeeVideoLink.Response> GetEmployeeVideoLink([FromRoute] int EmployeeId)
    {
        var request = new GetEmployeeVideoLink.Request { EmployeeId = EmployeeId };
        var response = await Svc<GetEmployeeVideoLink>().InvokeNoTrackingAsync(request);
        return response;
    }

    [HttpPost("employee/introduction")]
    public async Task<SetEmployeeIntroduction.Response> SetEmployeeIntroduction([FromBody] SetEmployeeIntroduction.Request request)
    {
        var response = await Svc<SetEmployeeIntroduction>().InvokeAsync(request);
        return response;
    }

    [HttpGet("employee/introduction/{EmployeeId}")]
    public async Task<GetEmployeeIntroduction.Response> GetEmployeeIntroduction([FromRoute] int EmployeeId)
    {
        var request = new GetEmployeeIntroduction.Request { EmployeeId = EmployeeId };
        var response = await Svc<GetEmployeeIntroduction>().InvokeNoTrackingAsync(request);
        return response;
    }

    [HttpPost("employee/social-media-details")]
    public async Task<SetEmployeeSocialMediaDetails.Response> SetEmployeeSocialMediaDetails([FromBody] SetEmployeeSocialMediaDetails.Request request)
    {
        var response = await Svc<SetEmployeeSocialMediaDetails>().InvokeAsync(request);
        return response;
    }

    [HttpGet("employee/social-media-details/{EmployeeId}")]
    public async Task<GetEmployeeSocialMediaDetails.Response> GetEmployeeSocialMediaDetails([FromRoute] int EmployeeId)
    {
        var request = new GetEmployeeSocialMediaDetails.Request { EmployeeId = EmployeeId };
        var response = await Svc<GetEmployeeSocialMediaDetails>().InvokeNoTrackingAsync(request);
        return response;
    }

    [HttpPost("employee/photo/upload")]
    public async Task<SetEmployeePhoto.Response> SetEmployeePhoto([FromForm] SetEmployeePhoto.Request request)
    {
        request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString().Replace("::1", "185.49.46.124");
        var response = await Svc<SetEmployeePhoto>().InvokeAsync(request);
        return response;
    }

    [HttpGet("employee/photo/{EmployeeId}")]
    public async Task<GetEmployeePhoto.Response> GetEmployeePhoto([FromRoute] int EmployeeId)
    {
        var request = new GetEmployeePhoto.Request { EmployeeId = EmployeeId };
        var response = await Svc<GetEmployeePhoto>().InvokeNoTrackingAsync(request);
        return response;
    }
}