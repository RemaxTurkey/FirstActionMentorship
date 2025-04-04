using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers.Base;
using API.Extensions;
using Application.Services.Common;
using Application.Services.Mail;
using Application.Services.Neighbor;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/")]
    public class HomeController : ApiControllerBase
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("{employeeId}/AcceptTerms")]
        public async Task<IActionResult> AcceptTerms([FromRoute] int employeeId)
        {
            var response = await Svc<AcceptTerms>().InvokeAsync(new AcceptTerms.Request(employeeId));
            return Ok(response);
        }

        [HttpGet("{employeeId}/AcceptanceStatus")]
        public async Task<IActionResult> GetAcceptanceStatus([FromRoute] int employeeId)
        {
            var response = await Svc<GetAcceptanceStatus>().InvokeAsync(new GetAcceptanceStatus.Request(employeeId));
            return Ok(response);
        }

        [HttpGet("template/{id}/employee/{employeeId}/type/{typeId}")]
        public async Task<IActionResult> GetTemplate([FromRoute] int id, [FromRoute] int employeeId, [FromRoute] int typeId)
        {
            var ext = typeId == 1 ? ".docx" : ".pdf";

            var path = string.Concat(_configuration.GetValue<string>("AppSettings:FAMFileUploadPath"), "Documents",
                _configuration.GetValue<string>("AppSettings:FAMFileUploadPath").Contains("/") ? "/" : "\\");
            var fileName = string.Concat(Guid.NewGuid().ToString().Replace("-", ""), ext);
            var serverName = CommonFunctions.CreateFileName(path, fileName);
            
            var response = await Svc<GetTemplate>().InvokeAsync(new GetTemplate.Request(id, employeeId, typeId));
            await System.IO.File.WriteAllBytesAsync(path + serverName, response);
            return Ok(string.Concat(_configuration.GetValue<string>("AppSettings:FAMFileUploadUrl"), "Documents", "/", serverName.Replace("\\", "/")));
        }

        [HttpPost("SendAcceptanceNotification")]
        public async Task<IActionResult> SendAcceptanceNotification([FromBody] FAMAcceptanceNotification.Request request)
        {
            var response = await Svc<FAMAcceptanceNotification>().InvokeAsync(request);
            return Ok(response);
        }
    }
}