using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QuoridorBackend.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger instance.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to log requests and responses.
    /// </summary>
    /// <param name="context">HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        
        context.Response.Headers["X-Request-Id"] = requestId;

        await LogRequest(context, requestId);

        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            sw.Stop();

            await LogResponse(context, requestId, sw.ElapsedMilliseconds);

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, 
                "Exception in Request {RequestId} - {Method} {Path} - Duration: {Duration}ms",
                requestId, context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
        
        var requestLog = new StringBuilder();
        requestLog.AppendLine($"Request {requestId} started");
        requestLog.AppendLine($"Method: {request.Method}");
        requestLog.AppendLine($"Path: {request.Path}");
        requestLog.AppendLine($"QueryString: {request.QueryString}");

        if (request.ContentLength > 0 && 
            (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH"))
        {
            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = await request.Body.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead));
                if (bytesRead == 0)
                {
                    break;
                }
                totalRead += bytesRead;
            }
            var bodyAsText = Encoding.UTF8.GetString(buffer, 0, totalRead);
            requestLog.AppendLine($"Body: {bodyAsText}");
            request.Body.Position = 0;
        }

        _logger.LogInformation(requestLog.ToString());
    }

    private async Task LogResponse(HttpContext context, string requestId, long duration)
    {
        var response = context.Response;
        
        response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "Request {RequestId} completed - Status: {StatusCode} - Duration: {Duration}ms - Body: {ResponseBody}",
            requestId, response.StatusCode, duration, responseBody);
    }
}