using System; // Required for AppDomain and Environment
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

/// <summary>
/// Inspired by Hans :) Thanks
/// </summary>
/// <param name="options"></param>
public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<ImageEntity> Images { get; set; }

    /*
        # Don't forget to Update the tool first in OS
        dotnet tool update --global dotnet-ef

        # Add migration
        dotnet ef migrations add InitialMigration --project Infrastructure --context DataContext --startup-project CloakVision.csproj

        # Update database
        dotnet ef database update --project Infrastructure --context DataContext --startup-project CloakVision.csproj

     */

    // Hans Recommended this when we want to use Lazyloading public virtual...

    // This will be added as last after Many-to-many relationships has been created
    // for sample Project and Services,

    /// <summary>
    /// Configures the database options for the current DbContext instance.
    /// This method is used to set provider-specific options, enable or disable features
    /// like lazy loading, caching, and to configure sensitive data logging for developer purposes.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to configure options for the DbContext.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only configure if not already configured (e.g., by DI if this DbContext is used in an app)
        if (optionsBuilder.IsConfigured)
            return;
        // Using Logs this to log sensitive data for developers
        optionsBuilder.EnableSensitiveDataLogging();
        // We can also enable caching here
        optionsBuilder.EnableServiceProviderCaching();

        // Enable lazy loading
        // Microsoft.EntityFrameworkCore.Proxies
        optionsBuilder.UseLazyLoadingProxies();
    }

    /// <summary>
    /// This is needed if we have multiple keys in a table to add the composite key (Two Primary Keys)
    /// Then this is needed to be added to the OnModelCreating. What should I do without Hans? :)
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Loading the configurations from the DataSeeding folder to seed the database
        // Inspired by Robin and Partially created by AI Phind
        // If you start an fresh project, you can comment this out
        // before you have any seeded data
        //  modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
