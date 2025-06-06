using Core.Factories.Data;
using Core.Factories.DTO;
using Core.Interfaces;
using Core.Interfaces.Factories;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

/// <summary>
/// This is a Composition Root pattern
/// https://medium.com/@cfryerdev/dependency-injection-composition-root-418a1bb19130
/// API -> Core
/// Infrastructure -> Core
/// API -> Infrastructure (only for DI registration)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IRepositoryResultFactory, RepositoryResultFactory>();
        services.AddScoped<IImageService, ImageService>();

        // Register factories
        services.AddScoped<IImageDtoFactory, ImageDtoFactory>();

        // Returning the services so it could be used
        // in our API layer where we are registering the services
        // using a Composition Root pattern
        return services;
    }
}
