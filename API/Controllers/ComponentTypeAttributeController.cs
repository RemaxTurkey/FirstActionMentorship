using API.Controllers.Base;
using Application.Services.ComponentTypeAttribute;
using Application.Services.ComponentTypeAttribute.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("api/componentTypeAttributes")]
[Produces("application/json")]
public class ComponentTypeAttributeController : ApiControllerBase
{
    /// <summary>
    /// Yeni bir bileşen tipi özelliği tanımlar
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile sisteme yeni bir bileşen tipi özelliği tanımlayabilirsiniz. 
    /// Örnek olarak "Text", "ImageUrl", "ButtonAction" gibi özellikler tanımlanabilir.
    /// Bu özellikler daha sonra belirli bileşen tiplerine atanabilir.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/componentTypeAttributes
    ///     {
    ///        "name": "Text",
    ///        "value": ""
    ///     }
    /// </remarks>
    /// <param name="request">Özellik bilgileri</param>
    /// <response code="201">Özellik başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComponentTypeAttributeDto>> CreateComponentTypeAttribute([FromBody] CreateComponentTypeAttribute.Request request)
    {
        var result = await Svc<CreateComponentTypeAttribute>().InvokeAsync(request);
        return Created($"api/componentTypeAttributes/{result.Item.Id}", result.Item);
    }
} 