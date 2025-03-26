using API.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }

    public static class CommonFunctions
    {
        public static string CreateFileName(string path, string fileName)
        {
            string separator = path.Contains("/") ? "/" : "\\";
            string addPath = String.Concat(DateTime.Now.Year.ToString(), separator, DateTime.Now.Month.ToString().PadLeft(2, '0'), separator, DateTime.Now.Day.ToString().PadLeft(2, '0'), separator);
            if (!System.IO.Directory.Exists(String.Concat(path, addPath))) System.IO.Directory.CreateDirectory(String.Concat(path, addPath));

            return String.Concat(addPath, Guid.NewGuid().ToString().Replace("-", ""), ".", fileName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last());
        }
    }
}