using API.Controllers.Base;
using API.Filters;
using Application.Services.ComponentType;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiResponse]
public class ComponentTypeController : ApiControllerBase
{
    [HttpGet]
    public async Task<GetComponentTypes.Response> GetComponentTypes([FromQuery] GetComponentTypes.Request request)
    {
        var response = await Svc<GetComponentTypes>().InvokeNoTrackingAsync(request);

        return response;
    }

    [HttpGet("{Id}")]
    public async Task<GetComponentType.Response> GetComponentType([FromRoute] GetComponentType.Request request)
    {
        var response = await Svc<GetComponentType>().InvokeNoTrackingAsync(request);

        return response;
    }

    [HttpPost]
    public async Task<CreateComponentType.Response> CreateComponentType([FromBody] CreateComponentType.Request request)
    {
        var response = await Svc<CreateComponentType>().InvokeAsync(request);

        return response;
    }

    [HttpPut("{Id}")]
    public async Task<UpdateComponentType.Response> UpdateComponentType([FromBody] UpdateComponentType.Request request)
    {
        var response = await Svc<UpdateComponentType>().InvokeAsync(request);

        return response;
    }
    
    [HttpDelete("{Id}")]
    public async Task<DeleteComponentType.Response> DeleteComponentType([FromRoute] DeleteComponentType.Request request)
    {
        var response = await Svc<DeleteComponentType>().InvokeAsync(request);

        return response;
    }    
} 