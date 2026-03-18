namespace Shared.Events;

public enum AppLogLevel { Info, Warning, Error, Critical }

public class LogEventMessage
{
    public string ServiceName { get; set; } = string.Empty;
    public AppLogLevel LogLevel { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}