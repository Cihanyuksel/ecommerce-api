using MassTransit;
using Shared.Events;
namespace Log.API.Consumers;

public class LogMessageConsumer : IConsumer<LogEventMessage>
{
    private readonly ILogger<LogMessageConsumer> _logger;

    public LogMessageConsumer(ILogger<LogMessageConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<LogEventMessage> context)
    {
        var msg = context.Message;

        var logTemplate = "[{ServiceName}] [{CorrelationId}] {Message} | Exception: {ExceptionMessage} | StackTrace: {StackTrace}";

        switch (msg.LogLevel)
        {
            case AppLogLevel.Info:
                _logger.LogInformation(logTemplate, msg.ServiceName, msg.CorrelationId, msg.Message, msg.ExceptionMessage, msg.StackTrace);
                break;
            case AppLogLevel.Warning:
                _logger.LogWarning(logTemplate, msg.ServiceName, msg.CorrelationId, msg.Message, msg.ExceptionMessage, msg.StackTrace);
                break;
            case AppLogLevel.Error:
                _logger.LogError(logTemplate, msg.ServiceName, msg.CorrelationId, msg.Message, msg.ExceptionMessage, msg.StackTrace);
                break;
            case AppLogLevel.Critical:
                _logger.LogCritical(logTemplate, msg.ServiceName, msg.CorrelationId, msg.Message, msg.ExceptionMessage, msg.StackTrace);
                break;
            default:
                _logger.LogInformation(logTemplate, msg.ServiceName, msg.CorrelationId, msg.Message, msg.ExceptionMessage, msg.StackTrace);
                break;
        }

        return Task.CompletedTask;
    }
}