using System.Linq.Expressions;
using Core.Interfaces.Data;
using Core.Interfaces.Factories;
using Domain;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

/// <summary>
/// Base repository implementation following Clean Architecture principles
/// Inspired By: Hans's tutorial (https://www.youtube.com/watch?v=lsEEYvCtFi4)
/// Key Features:
/// 1. Generic CRUD operations for domain-driven design
/// 2. Automatic domain/entity conversion via factory pattern
/// 3. Transaction management delegation to UnitOfWork
/// 4. Clear layer separation (Core never references EF entities)
/// </summary>
/// <remarks>
/// Implementation notes:
/// - Uses <see cref="IEntityFactory{TDomain,TEntity}"/> for bidirectional conversions
/// - All database operations are async-first
/// - Predicate conversion handles domain-to-entity type translation
/// - Designed for extension not modification (open/closed principle)
/// </remarks>
public abstract class BaseRepository<TDomain, TEntity>(
    DataContext dataContext,
    IEntityFactory<TDomain?, TEntity> factory,
    IRepositoryResultFactory resultFactory
) : IBaseRepository<TDomain>
    where TDomain : class
    where TEntity : class
{
    /// <summary>
    /// Represents the DbSet for the repository, providing access to the collection of entities in the database
    /// corresponding to the type <typeparamref name="TEntity"/>. Used to perform CRUD operations on the
    /// specific type of entity in the underlying database context.
    /// </summary>
    private readonly DbSet<TEntity> _dbSet = dataContext.Set<TEntity>();

    /// <summary>
    /// Creates a new domain entity and persists it in the repository.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be created and stored in the
    /// repository. This entity represents the business object to be processed.</param>
    /// <returns>A repository result containing the successfully created domain entity
    /// if the operation succeeds, or an error if the operation fails. The result also
    /// includes relevant HTTP status codes such as 201 for success or 400/500 for errors.</returns>
    public virtual async Task<RepositoryResult<TDomain?>> CreateAsync(TDomain? domainEntity)
    {
        // Domain Input validation
        if (domainEntity is null)
            // Return a failed result with a null value error
            return resultFactory.OperationFailed<TDomain?>(Error.NullValue, 400);
        try
        {
            // 1. Convert the domain object to an entity object
            var entity = factory.ToEntity(domainEntity);

            // Check if the conversion was successful
            if (entity is null)
                return resultFactory.OperationFailed<TDomain?>(
                    new Error(
                        "Repository.OperationFailed",
                        $"Failed to convert domain entity to entity: {domainEntity}"
                    ),
                    500
                );
            // 2. Add the entity to the DbSet and save changes
            await _dbSet.AddAsync(entity);

            // 3. Convert it back to a domain object
            var createdDomainEntity = factory.ToDomain(entity);
            if (createdDomainEntity is null)
                return resultFactory.OperationFailed<TDomain?>(
                    new Error(
                        "Repository.OperationFailed",
                        $"Failed to convert entity to domain entity: {entity}"
                    ),
                    500
                );
            // Return the created domain object and success result
            return resultFactory.OperationSuccess<TDomain?>(createdDomainEntity, 201);
        }
        catch (Exception ex)
        {
            // Log the exception
            return resultFactory.OperationFailed<TDomain?>(
                new Error("Repository.OperationFailed", ex.Message),
                500
            );
        }
    }

    /// <summary>
    /// Retrieves a single domain entity that matches the specified predicate.
    /// </summary>
    /// <param name="domainPredicate">The predicate expression used to filter the desired domain entity.</param>
    /// <returns>The matching domain entity, or null if no entity satisfies the predicate.</returns>
    public virtual async Task<RepositoryResult<TDomain?>> GetAsync(
        Expression<Func<TDomain?, bool>> domainPredicate
    )
    {
        return await GetAsync(domainPredicate, false, null);
    }

    /// <summary>
    /// Retrieves all domain entities that satisfy the specified predicate.
    /// </summary>
    /// <param name="domainPredicate">The predicate to filter the domain entities.</param>
    /// <returns>An asynchronous operation containing a collection of domain entities that match the predicate.</returns>
    public virtual async Task<IEnumerable<TDomain?>> GetAllAsync(
        Expression<Func<TDomain?, bool>> domainPredicate
    )
    {
        return await GetAllAsync(domainPredicate, includes: null);
    }

    /// <summary>
    /// Retrieves a domain entity that matches the specified predicate.
    /// </summary>
    /// <param name="domainPredicate">The predicate used to filter the domain entities.</param>
    /// <param name="tracking"></param>
    /// <param name="includes"></param>
    /// <returns>The domain entity that matches the predicate or null if no match is found.</returns>
    public virtual async Task<RepositoryResult<TDomain?>> GetAsync(
        Expression<Func<TDomain?, bool>> domainPredicate,
        bool tracking = false,
        params Expression<Func<TDomain, object>>[]? includes
    )
    {
        try
        {
            // Convert the domain predicate to an entity predicate
            var entityIncludes = includes
                ?.Select(include =>
                    factory.CreateEntityInclude(include!)
                    ?? throw new InvalidOperationException($"Include mapping missing for {include}")
                )
                .ToArray();

            // Create a query from the DbSet
            var query = _dbSet.AsQueryable();

            // Apply includes BEFORE tracking
            if (entityIncludes is { Length: > 0 })
                query = entityIncludes.Aggregate(
                    query,
                    (current, include) => current.Include(include)
                );

            // Convert predicate AFTER includes
            var entityPredicate = factory.CreateEntityPredicate(domainPredicate);

            // Apply tracking LAST
            query = tracking ? query.AsTracking() : query.AsNoTracking();

            // Apply the entity predicate to the query
            var entity = await query.FirstOrDefaultAsync(entityPredicate);

            // If no entity is found, return a not found result
            if (entity == null)
                return resultFactory.OperationSuccess<TDomain?>(null, 404);

            var domainEntity = factory.ToDomain(entity);
            if (domainEntity == null)
                return resultFactory.OperationFailed<TDomain?>(
                    new Error(
                        "Repository.OperationFailed",
                        $"Failed to convert entity to domain entity: {entity}"
                    ),
                    500
                );

            return resultFactory.OperationSuccess<TDomain?>(domainEntity, 200);
        }
        catch (InvalidOperationException ex)
        {
            return resultFactory.OperationFailed<TDomain?>(
                new Error("Repository.OperationInvalid", ex.Message),
                400
            );
        }
        catch (Exception ex)
        {
            // Log the exception
            return resultFactory.OperationFailed<TDomain?>(
                new Error("Repository.OperationFailed", ex.Message),
                500
            );
        }
    }

    /// <summary>
    /// Retrieves all domain entities that satisfy the specified predicate, with optional tracking and inclusion of related entities.
    /// </summary>
    /// <param name="domainPredicate">The predicate used to filter the domain entities.</param>
    /// <param name="tracking">Indicates whether the context should track the retrieved entities. Defaults to false.</param>
    /// <param name="includes">Optional navigation properties to include in the query results.</param>
    /// <returns>A collection of domain entities that match the specified predicate.</returns>
    public virtual async Task<IEnumerable<TDomain?>> GetAllAsync(
        Expression<Func<TDomain?, bool>> domainPredicate,
        bool tracking = false,
        params Expression<Func<TDomain, object>>[]? includes
    )
    {
        // Convert the domain predicate to an entity predicate
        var entityPredicate = factory.CreateEntityPredicate(domainPredicate);

        // Create the query
        var query = _dbSet.IncludeNavigationProperties(dataContext);

        // Check if tracking is enabled
        query = tracking ? query.AsTracking() : query.AsNoTracking();

        // Apply the entity predicate
        query = query.Where(entityPredicate);

        // Check if includes are provided
        if (includes != null)
            // Include related entities
            query = includes
                .Select(factory.CreateEntityInclude!)
                .Aggregate(query, (current, entityInclude) => current.Include(entityInclude));

        // Get all entities that match the predicate
        var entity = await query.ToListAsync();
        // Got Partial help from Phind AI to finish this Line
        return entity.Select(e => factory.ToDomain(e)!);
    }

    /// <summary>
    /// Updates an existing domain entity by converting it to an entity object, marking it as modified,
    /// and reflecting the changes in the database.
    /// </summary>
    /// <param name="domainEntity">The domain entity to update.</param>
    /// <returns>The updated domain entity, or null if the operation failed.</returns>
    public virtual Task<TDomain?> UpdateAsync(TDomain? domainEntity)
    {
        // Convert the domain object to an entity object
        var entity = factory.ToEntity(domainEntity);

        // Attach and mark as unchanged to avoid tracking conflicts
        _dbSet.Attach(entity ?? throw new InvalidOperationException());

        // Mark the entity as modified
        dataContext.Entry(entity).State = EntityState.Modified;

        // Return the domain object
        return Task.FromResult(factory.ToDomain(entity));
    }

    /// <summary>
    /// Deletes an existing domain entity by removing it from the database.
    /// </summary>
    /// <param name="domainEntity">The domain entity to delete.</param>
    /// <returns>The deleted domain entity, or null if the operation failed.</returns>
    public virtual Task<TDomain?> DeleteAsync(TDomain? domainEntity)
    {
        // Convert the domain object to an entity object
        var entity = factory.ToEntity(domainEntity);

        // Remove the entity from the DbSet
        _dbSet.Remove(entity ?? throw new InvalidOperationException());

        // Return the domain object
        return Task.FromResult(factory.ToDomain(entity));
    }

    /// <summary>
    /// Attaches a domain entity to the current context, enabling its tracking without persisting any changes.
    /// </summary>
    /// <param name="domainEntity">The domain entity to attach to the current context.</param>
    /// <returns>The attached domain entity, or null if the conversion or operation failed.</returns>
    public Task<TDomain?> AttachAsync(TDomain? domainEntity)
    {
        var entity = factory.ToEntity(domainEntity);
        _dbSet.Attach(entity ?? throw new InvalidOperationException());
        return Task.FromResult(factory.ToDomain(entity));
    }

    /// <summary>
    /// Check if a domain entity exists using the database predicate
    /// using code inspired from this base repository building on Hans's tutorial.
    /// </summary>
    /// <param name="domainPredicate"></param>
    /// <returns></returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<TDomain?, bool>> domainPredicate)
    {
        // Convert the domain predicate to an entity predicate (see if we have that entity in the database)
        var entityPredicate = factory.CreateEntityPredicate(domainPredicate);
        // Return the result of the operation
        return await _dbSet.AnyAsync(entityPredicate);
    }

    /// <summary>
    /// Retrieves a domain entity if it exists, matching the specified predicate.
    /// </summary>
    /// <param name="domainPredicate">The predicate to match a domain entity.</param>
    /// <returns>The matched domain entity, or null if none exists.</returns>
    public virtual async Task<TDomain?> GetIfExistsAsync(
        Expression<Func<TDomain?, bool>> domainPredicate
    )
    {
        // Convert the domain predicate to an entity predicate using the factory
        var entityPredicate = factory.CreateEntityPredicate(domainPredicate);
        // Get the first entity that matches the predicate
        var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(entityPredicate);
        // Return the domain object using the factory because we want to return the domain object
        return entity != null ? factory.ToDomain(entity) : null;
    }
}

/// <summary>
/// Provides extension methods for IQueryable to facilitate operations with navigation properties in the context of Entity Framework.
/// </summary>
/// <remarks>
/// Key Features:
/// - Enhances IQueryable by enabling automatic inclusion of navigation properties.
/// - Primarily intended to reduce boilerplate when working with complex relationships in an EF DbContext.
/// - Designed for use with Entity Framework Core's DbContext.
/// Implementation Notes:
/// - Utilizes the DbContext Metadata API to retrieve navigation properties of the entity type.
/// - Aggregates navigation properties using Entity Framework Core's <see /> method.
/// - Ensures extensibility as it works generically with any TEntity.
/// </remarks>
public static class QueryableExtensions
{
    public static IQueryable<TEntity> IncludeNavigationProperties<TEntity>(
        this IQueryable<TEntity> query,
        DbContext context
    )
        where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var navigations = entityType?.GetNavigations() ?? [];

        return navigations.Aggregate(
            query,
            (current, navigation) => current.Include(navigation.Name)
        );
    }
}
