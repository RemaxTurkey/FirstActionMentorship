using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers.Base;
using Application.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/")]
    public class HomeController : ApiControllerBase
    {
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
    }
}