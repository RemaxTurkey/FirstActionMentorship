using API.Controllers.Base;
using API.Filters;
using Application.Services.ContentCategory;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiResponse]
public class ContentCategoryController : ApiControllerBase
{
    [HttpGet]
    public async Task<GetContentCategories.Response> GetContentCategories([FromQuery] GetContentCategories.Request request)
    {
        var response = await Svc<GetContentCategories>().InvokeNoTrackingAsync(request);

        return response;
    }

    [HttpGet("{Id}")]
    public async Task<GetContentCategory.Response> GetContentCategory([FromRoute] GetContentCategory.Request request)
    {
        var response = await Svc<GetContentCategory>().InvokeNoTrackingAsync(request);

        return response;
    }

    [HttpPost]
    public async Task<CreateContentCategory.Response> CreateContentCategory([FromBody] CreateContentCategory.Request request)
    {
        var response = await Svc<CreateContentCategory>().InvokeAsync(request);

        return response;
    }

    [HttpPut("{Id}")]
    public async Task<UpdateContentCategory.Response> UpdateContentCategory([FromBody] UpdateContentCategory.Request request)
    {
        var response = await Svc<UpdateContentCategory>().InvokeAsync(request);

        return response;
    }
    
    [HttpDelete("{Id}")]
    public async Task<DeleteContentCategory.Response> DeleteContentCategory([FromRoute] DeleteContentCategory.Request request)
    {
        var response = await Svc<DeleteContentCategory>().InvokeAsync(request);

        return response;
    }    
}