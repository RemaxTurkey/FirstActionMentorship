using API.Controllers.Base;
using Application.Services.Component;
using Application.Services.Component.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("api/components")]
[Produces("application/json")]
public class ComponentController : ApiControllerBase
{
    /// <summary>
    /// Yeni bir bileşen oluşturur
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile sisteme yeni bir bileşen ekleyebilirsiniz.
    /// Her bileşen bir ComponentType'a bağlıdır ve o type'ın izin verdiği özelliklere (attributes) sahip olabilir.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/components
    ///     {
    ///        "componentTypeId": 1,
    ///        "isActive": true,
    ///        "componentAttributes": [
    ///           {
    ///              "componentTypeAttributeId": 1,
    ///              "value": "Menüye Git"
    ///           },
    ///           {
    ///              "componentTypeAttributeId": 2,
    ///              "value": "/menu"
    ///           }
    ///        ],
    ///        "componentItems": []
    ///     }
    /// </remarks>
    /// <param name="request">Bileşen bilgileri</param>
    /// <response code="200">Bileşen başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">ComponentType bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComponentDto>> CreateComponent([FromBody] CreateComponent.Request request)
    {
        var result = await Svc<CreateComponent>().InvokeAsync(request);
        return Created($"api/components/{result.Item.Id}", result.Item);
    }

    /// <summary>
    /// Bir bileşene yeni özellik değeri ekler
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile mevcut bir bileşene, bileşen tipine uygun yeni bir özellik değeri ekleyebilirsiniz.
    /// Eklenen özellik, bileşen tipinin desteklediği özelliklerden biri olmalıdır.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/components/1/items
    ///     {
    ///        "componentTypeId": 1,
    ///        "item": {
    ///           "attributeId": 3,
    ///           "value": "#FF5733"
    ///        }
    ///     }
    /// </remarks>
    /// <param name="componentId">Bileşen ID</param>
    /// <param name="request">Özellik değeri bilgileri</param>
    /// <response code="201">Özellik değeri başarıyla eklendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Bileşen veya özellik bulunamadı</response>
    [HttpPost("{componentId}/items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> AddComponentItem(int componentId, [FromBody] AddComponentItem.Request request)
    {
        // Request değerini güncelle
        request.ComponentId = componentId;
        
        var result = await Svc<AddComponentItem>().InvokeAsync(request);
        return Created($"api/components/{componentId}/items", result.Success);
    }
} 