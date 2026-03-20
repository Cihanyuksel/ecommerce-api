using Shared.Events;
using MassTransit;
using System.Net;
using System.Text.Json;
using Product.Domain.Exceptions;

namespace Product.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        var logLevel = ex switch
        {
            NotFoundException => AppLogLevel.Error,
            BadRequestException => AppLogLevel.Error,
            ValidationException => AppLogLevel.Warning,
            _ => AppLogLevel.Error
        };

        var publishEndpoint = context.RequestServices.GetRequiredService<IPublishEndpoint>();

        await publishEndpoint.Publish(new LogEventMessage
        {
            ServiceName = "Product.API",
            LogLevel = logLevel,
            Message = $"İşlem başarısız: {context.Request.Method} {context.Request.Path}",
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        });

        var statusCode = ex switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            BadRequestException => HttpStatusCode.BadRequest,
            ValidationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message, correlationId }));
    }
}