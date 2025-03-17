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
    public async Task<GetContent.Response> GetContent([FromRoute] GetContent.Request request)
    {
        var response = await Svc<GetContent>().InvokeNoTrackingAsync(request);
        return response;
    }

    [HttpPost("save/{SaveContentId}/{NextContentId}")]
    public async Task<SaveAndGetNextContent.Response> SaveAndGetNextContent([FromRoute] SaveAndGetNextContent.Request request)
    {
        var response = await Svc<SaveAndGetNextContent>().InvokeAsync(request);
        return response;
    }
}