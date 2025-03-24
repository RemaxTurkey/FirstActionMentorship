using API.Controllers.Base;
using Application.Services.Component;
using Application.Services.Component.DTOs;
using Application.UnitOfWorks;
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
    /// Bir bileşen bilgilerini getirir
    /// </summary>
    /// <param name="request">Bileşen bilgileri</param>
    /// <response code="200">Bileşen başarıyla getirildi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Bileşen bulunamadı</response>
    [HttpGet("{Id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<GetComponent.Response> GetComponent([FromRoute] GetComponent.Request request)
    {
        var response = await Svc<GetComponent>().InvokeNoTrackingAsync(request);
        return response;
    }

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
    ///        "componentTypeAttributeValues": [
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
    public async Task<CreateComponent.Response> CreateComponent([FromBody] CreateComponent.Request request)
    {
        var response = await Svc<CreateComponent>().InvokeAsync(request);
        return response;
    }

    /// <summary>
    /// Bir bileşen bilgilerini günceller
    /// </summary>
    /// <param name="Id">Bileşen ID</param>
    /// <param name="request">Bileşen bilgileri</param>
    /// <response code="200">Bileşen başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Bileşen bulunamadı</response>
    [HttpPut("{Id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<UpdateComponent.Response> UpdateComponent([FromRoute] int Id, [FromBody] UpdateComponent.Request request)
    {
        request.Id = Id;
        var response = await Svc<UpdateComponent>().InvokeAsync(request);
        return response;
    }

    /// <summary>
    /// Birden fazla bileşen bilgilerini günceller
    /// </summary>
    /// <param name="request">Bileşenlerin bilgileri</param>
    /// <response code="200">Bileşenler başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Bileşen bulunamadı</response>
    [HttpPut("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<UpdateComponentBatch.Response> UpdateComponentBatch([FromBody] UpdateComponentBatch.Request request)
    {
        var response = await Svc<UpdateComponentBatch>().InvokeAsync(request);
        return response;
    }
} 