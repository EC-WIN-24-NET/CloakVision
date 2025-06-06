using Core.Interfaces.Data;
using Core.Interfaces.Factories;
using Domain;
using Infrastructure.Contexts;
using Infrastructure.Entities;
using Infrastructure.Factories;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Provides methods for registering infrastructure layer dependencies into
/// the ASP.NET Core dependency injection container.
/// </summary>
/// <remarks>
/// This static class simplifies configuring the infrastructure dependencies such as
/// database contexts, repositories, and services. It ensures that all required dependencies
/// for the infrastructure layer are properly registered in the application's
/// IServiceCollection. The main focus is on handling data access and basic
/// domain data management.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the required services and configurations for the infrastructure layer.
    /// This includes setting up the database context and dependency injection for
    /// infrastructure-specific functionalities such as the UnitOfWork.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the dependencies will be added.</param>
    /// <param name="connectionString">The connection string used to configure the database context.</param>
    /// <returns>The updated IServiceCollection with the infrastructure dependencies added.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        // We are taking the connection string from the API/Presentation layer.
        services.AddDbContext<DataContext>(options =>
            options.UseAzureSql(
                connectionString,
                b => b.MigrationsAssembly(typeof(DataContext).Assembly.FullName)
            )
        );

        // Registering the repositories in a DI container
        services.AddScoped<IImageRepository, ImageRepository>();

        // Registering the factories in a DI container
        services.AddScoped<IEntityFactory<Image, ImageEntity>, ImageFactory>();

        return services;
    }
}
