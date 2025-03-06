using API.Controllers.Base;
using Application.Services.ComponentTypeAttributeAssoc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("api/componentTypeAttributeAssocs")]
[Produces("application/json")]
public class ComponentTypeAttributeAssocController : ApiControllerBase
{
    /// <summary>
    /// Bir bileşen tipi ile özellik arasında ilişki kurar
    /// </summary>
    /// <remarks>
    /// Bu endpoint ile bir bileşen tipinin hangi özelliklere sahip olabileceğini tanımlarsınız.
    /// Örneğin, "Menu Item" tipindeki bir bileşenin "Text", "Icon" ve "Action" özelliklerine sahip olmasını istiyorsanız,
    /// her bir özellik için ayrı ilişki kurmanız gerekir.
    /// 
    /// Örnek istek:
    /// 
    ///     POST /api/componentTypeAttributeAssocs
    ///     {
    ///        "componentTypeId": 1,
    ///        "componentTypeAttributeId": 2
    ///     }
    /// </remarks>
    /// <param name="request">İlişki bilgileri</param>
    /// <response code="201">İlişki başarıyla kuruldu</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Bileşen tipi veya özellik bulunamadı</response>
    /// <response code="409">Bu ilişki zaten mevcut</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<bool>> AssignAttributeToComponentType([FromBody] AssignAttributeToComponentType.Request request)
    {
        var result = await Svc<AssignAttributeToComponentType>().InvokeAsync(request);
        return Created($"api/componentTypeAttributeAssocs/{request.ComponentTypeId}/{request.ComponentTypeAttributeId}", result.Success);
    }
} 