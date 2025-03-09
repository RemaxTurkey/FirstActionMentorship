using API.Controllers.Base;
using Application.Services.Component;
using Application.Services.Component.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("api/componentTypeAttributeValues")]
[Produces("application/json")]
public class ComponentTypeAttributeValueController : ApiControllerBase
{
    /// <summary>
    /// Bir component'a ComponentType'a ait bir attribute için değer ekler veya günceller
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile bir bileşene, bileşen tipinin izin verdiği özelliklerden birini ekleyebilir veya güncelleyebilirsiniz.
    /// Aynı özellik daha önce eklenmiş ise değeri güncellenir.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/componentTypeAttributeValues
    ///     {
    ///        "componentId": 1,
    ///        "componentTypeId": 1,
    ///        "attributeValue": {
    ///           "componentTypeAttributeId": 1,
    ///           "value": "Yeni Değer"
    ///        }
    ///     }
    /// </remarks>
    /// <param name="request">Özellik değeri bilgileri</param>
    /// <response code="200">Özellik değeri başarıyla eklendi/güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Component veya ComponentTypeAttribute bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> AddComponentTypeAttributeValue([FromBody] AddComponentTypeAttributeValue.Request request)
    {
        var result = await Svc<AddComponentTypeAttributeValue>().InvokeAsync(request);
        return Ok(result);
    }
} 