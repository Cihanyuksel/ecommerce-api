using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Product.API.Extensions;
using Product.API.Middleware;
using Product.Application.Extensions;
using Product.Infrastructure.Extensions;
using Product.Infrastructure.Persistence;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var publishEndpoint = services.GetRequiredService<IPublishEndpoint>();

    try
    {
        var context = services.GetRequiredService<ProductDbContext>();
        var canConnect = await context.Database.CanConnectAsync();

        if (!canConnect)
            throw new Exception("SQL Server yanıt vermiyor!");
    }
    catch (Exception ex)
    {
        await publishEndpoint.Publish(new LogEventMessage
        {
            ServiceName = "Product.API",
            LogLevel = AppLogLevel.Critical,
            Message = "CRITICAL: Product Veritabanına bağlanılamadı!",
            ExceptionMessage = ex.Message,
            Timestamp = DateTime.UtcNow
        });
        Console.WriteLine($" CRITICAL: Veritabanına ulaşılamıyor. {ex.Message}");
    }

    try
    {
        var cache = services.GetRequiredService<IDistributedCache>();
        await cache.GetStringAsync("ping_test");
    }
    catch (Exception ex)
    {
        await publishEndpoint.Publish(new LogEventMessage
        {
            ServiceName = "Product.API",
            LogLevel = AppLogLevel.Critical,
            Message = "CRITICAL: Redis Önbellek (Cache) sunucusuna bağlanılamadı!",
            ExceptionMessage = ex.Message,
            Timestamp = DateTime.UtcNow
        });
        Console.WriteLine($"Redis sunucusuna ulaşılamıyor! {ex.Message}");
    }
}

app.Run();