using System.Linq.Expressions;
using Domain;

namespace Core.Interfaces.Data;

/// <summary>
/// Defines a generic repository interface for handling CRUD operations and common data access methods
/// for a domain entity type.
/// </summary>
/// <typeparam name="TDomain">The type of the domain entity.</typeparam>
public interface IBaseRepository<TDomain>
    where TDomain : class
{
    Task<RepositoryResult<TDomain?>> CreateAsync(TDomain? domainEntity);
    Task<TDomain?> UpdateAsync(TDomain? domainEntity);

    Task<RepositoryResult<TDomain?>> GetAsync(
        Expression<Func<TDomain?, bool>> domainPredicate,
        bool tracking = false,
        params Expression<Func<TDomain, object>>[]? includes
    );

    Task<IEnumerable<TDomain?>> GetAllAsync(
        Expression<Func<TDomain?, bool>> domainPredicate,
        bool tracking = false,
        params Expression<Func<TDomain, object>>[]? includes
    );

    Task<TDomain?> DeleteAsync(TDomain? domainEntity);

    Task<TDomain?> AttachAsync(TDomain? domainEntity);

    // Section to check for data and return of existing
    Task<bool> AnyAsync(Expression<Func<TDomain?, bool>> domainPredicate);
    Task<TDomain?> GetIfExistsAsync(Expression<Func<TDomain?, bool>> domainPredicate);
}
