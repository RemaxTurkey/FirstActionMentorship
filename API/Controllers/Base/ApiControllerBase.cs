using API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected T Svc<T>()
    {
        return HttpContext.RequestServices.GetService<T>();
    }
}