using Microsoft.AspNetCore.Builder;

namespace QuoridorBackend.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
