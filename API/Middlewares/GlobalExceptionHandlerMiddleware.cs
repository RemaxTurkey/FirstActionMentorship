using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                BusinessException businessException =>
                    ApiResponse<object>.Error(businessException.Message),
                ArgumentNullException argumentNullException =>
                    ApiResponse<object>.Error(argumentNullException.Message),
                ArgumentException argumentException =>
                    ApiResponse<object>.Error(argumentException.Message),
                //_ => ApiResponse<object>.Error("Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.")
                _ => ApiResponse<object>.Error(exception.Message)
            };

            context.Response.StatusCode = exception switch
            {
                BusinessException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}