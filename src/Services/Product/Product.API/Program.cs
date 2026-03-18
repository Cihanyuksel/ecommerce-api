using Microsoft.EntityFrameworkCore;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Infrastructure.Persistence;
using Product.Application.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Shared.Events;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token'ınızı buraya yapıştırın."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApplicationDbContext, ProductDbContext>();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "ProductCache_";
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddMassTransit(x =>
{

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var username = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var password = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Product.API.Middleware.ExceptionMiddleware>();
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
        {
            throw new Exception("SQL Server yanıt vermiyor!");
        }
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
        Console.WriteLine($"💥 Kritik Hata: Veritabanına ulaşılamıyor. {ex.Message}");
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

