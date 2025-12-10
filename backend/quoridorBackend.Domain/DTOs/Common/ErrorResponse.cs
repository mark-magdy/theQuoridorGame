namespace QuoridorBackend.Domain.DTOs.Common;

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}
