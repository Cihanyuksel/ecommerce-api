using Auth.Application.Features.Auth.Commands.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

        return services;
    }
}