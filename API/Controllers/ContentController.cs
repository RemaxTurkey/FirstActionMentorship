using API.Controllers.Base;
using Application.Services.Content;
using Application.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ContentController : ApiControllerBase
{
    [HttpPost]
    public async Task<CreateContent.Response> CreateComponentType([FromBody] CreateContent.Request request)
    {
        var response = await Svc<CreateContent>().InvokeAsync(request);
        return response;
    }
    
    [HttpGet("{Id}")]
    public async Task<GetContent.Response> GetComponentType([FromRoute] GetContent.Request request)
    {
        var response = await Svc<GetContent>().InvokeNoTrackingAsync(request);
        return response;
    }
}