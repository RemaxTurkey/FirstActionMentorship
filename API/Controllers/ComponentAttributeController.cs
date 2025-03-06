using API.Controllers.Base;
using Application.Services.Component;
using Application.Services.Component.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("api/componentAttributes")]
[Produces("application/json")]
public class ComponentAttributeController : ApiControllerBase
{
    /// <summary>
    /// Bir component'a özellik değeri ekler veya günceller
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile bir bileşene, bileşen tipinin izin verdiği özelliklerden birini ekleyebilir veya güncelleyebilirsiniz.
    /// Aynı özellik daha önce eklenmiş ise değeri güncellenir.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/componentAttributes
    ///     {
    ///        "componentId": 1,
    ///        "componentTypeId": 1,
    ///        "attribute": {
    ///           "componentTypeAttributeId": 1,
    ///           "value": "Yeni Değer"
    ///        }
    ///     }
    /// </remarks>
    /// <param name="request">Özellik bilgileri</param>
    /// <response code="200">Özellik başarıyla eklendi/güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Component veya ComponentTypeAttribute bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> AddComponentAttribute([FromBody] AddComponentAttribute.Request request)
    {
        var result = await Svc<AddComponentAttribute>().InvokeAsync(request);
        return Ok(result);
    }
} 