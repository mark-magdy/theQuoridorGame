using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuoridorBackend.Api.Middleware.Exceptions;

using QuoridorBackend.Domain.DTOs.Common;

namespace QuoridorBackend.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        
        _logger.LogError(exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId, context.Request.Path, context.Request.Method);

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFound => 
                (HttpStatusCode.NotFound, notFound.Message, null),
            
            BadRequestException badRequest => 
                (HttpStatusCode.BadRequest, badRequest.Message, null),
            
            UnauthorizedException unauthorized => 
                (HttpStatusCode.Unauthorized, unauthorized.Message, null),
            
            ForbiddenException forbidden => 
                (HttpStatusCode.Forbidden, forbidden.Message, null),
            
            ValidationException validation => 
                (HttpStatusCode.BadRequest, validation.Message, validation.Errors),
            
            ArgumentException argumentEx => 
                (HttpStatusCode.BadRequest, argumentEx.Message, null),
            
            InvalidOperationException invalidOp => 
                (HttpStatusCode.BadRequest, invalidOp.Message, null),
            
            _ => 
                (HttpStatusCode.InternalServerError, "An internal server error occurred.", null)
        };

        var errorResponse = new ErrorResponse
        {
            Code = ((int)statusCode).ToString(),
            Message = message,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow,
            Errors = errors
        };

        if (_environment.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}
