using API.Controllers.Base;
using API.Filters;
using Application.Services.Component;
using Application.Services.ComponentType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiResponse]
[ApiController]
[Route("api/componentTypes")]
[Produces("application/json")]
public class ComponentTypeController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetComponentTypes.Request request)
    {
        var response = await Svc<GetComponentTypes>().InvokeAsync(request);
        return Ok(response);
    }

    [HttpGet("{Id}")]
    public async Task<GetComponentType.Response> GetComponentType([FromRoute] GetComponentType.Request request)
    {
        var response = await Svc<GetComponentType>().InvokeNoTrackingAsync(request);

        return response;
    }

    /// <summary>
    /// Yeni bir bileşen tipi oluşturur
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile sisteme yeni bir bileşen tipi ekleyebilirsiniz.
    /// Bileşen tipi oluşturulurken, ona ait özellikler (attributes) de aynı anda eklenebilir.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/componentTypes
    ///     {
    ///        "title": "Menu Item",
    ///        "attributes": [
    ///           {
    ///              "name": "Text",
    ///              "value": ""
    ///           },
    ///           {
    ///              "name": "Icon",
    ///              "value": ""
    ///           },
    ///           {
    ///              "name": "Action",
    ///              "value": ""
    ///           }
    ///        ]
    ///     }
    /// </remarks>
    /// <param name="request">Bileşen tipi bilgileri</param>
    /// <response code="200">Bileşen tipi başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek</response>
    [HttpPost]
    public async Task<CreateComponentType.Response> CreateComponentType([FromBody] CreateComponentType.Request request)
    {
        var response = await Svc<CreateComponentType>().InvokeAsync(request);

        return response;
    }

    /// <summary>
    /// Bir bileşen tipini günceller
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile mevcut bir bileşen tipini güncelleyebilir ve yeni özellikler ekleyebilirsiniz.
    /// Mevcut özellikler korunurken, yeni özellikler (id değeri olmayan) eklenebilir.
    /// 
    /// Örnek istek:
    /// 
    ///     PUT /api/componentTypes/1
    ///     {
    ///        "id": 1,
    ///        "title": "Updated Menu Item",
    ///        "attributes": [
    ///           {
    ///              "name": "NewAttribute",
    ///              "value": ""
    ///           }
    ///        ]
    ///     }
    /// </remarks>
    /// <param name="request">Güncellenmiş bileşen tipi bilgileri</param>
    /// <response code="200">Bileşen tipi başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Bileşen tipi bulunamadı</response>
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

    /// <summary>
    /// Bir bileşen tipine ait özellikleri getirir
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile belirli bir bileşen tipinin sahip olabileceği tüm özellikleri listeleyebilirsiniz.
    /// Bu liste, bileşen oluştururken hangi özelliklerin kullanılabileceğini belirler.
    /// 
    /// Örnek yanıt:
    /// 
    ///     {
    ///        "attributes": [
    ///           {
    ///              "id": 1,
    ///              "name": "Text",
    ///              "value": ""
    ///           },
    ///           {
    ///              "id": 2,
    ///              "name": "Icon",
    ///              "value": ""
    ///           }
    ///        ]
    ///     }
    /// </remarks>
    /// <param name="componentTypeId">Bileşen tipi ID</param>
    /// <response code="200">Özellikler başarıyla getirildi</response>
    /// <response code="404">Bileşen tipi bulunamadı</response>
    [HttpGet("{componentTypeId}/attributes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetComponentTypeAttributes.Response>> GetComponentTypeAttributes(int componentTypeId)
    {
        var request = new GetComponentTypeAttributes.Request { ComponentTypeId = componentTypeId };
        var result = await Svc<GetComponentTypeAttributes>().InvokeAsync(request);
        return Ok(result);
    }
} 