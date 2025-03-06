using System;
using System.Threading.Tasks;
using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiResponseAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Exception != null)
            {
                return;
            }

            if (resultContext.Result is not ObjectResult objectResult)
            {
                return;
            }

            if (objectResult.Value != null && 
                objectResult.Value.GetType().IsGenericType &&
                objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                return;
            }
            
            if (objectResult.Value != null)
            {
                dynamic response = typeof(ApiResponse<>)
                    .MakeGenericType(objectResult.Value.GetType())
                    .GetMethod("Success")
                    .Invoke(null, new[] { objectResult.Value, null });

                objectResult.Value = response;
            }
        }
    }
} 