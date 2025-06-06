using System.Linq.Expressions;

namespace Core.Interfaces.Factories;

public interface IEntityFactory<TDomain, TEntity>
{
    // Convert a domain predicate to an entity predicate
    // Used AI Phind to complete this line
    Expression<Func<TEntity, bool>> CreateEntityPredicate(
        Expression<Func<TDomain, bool>>? domainPredicate
    );

    // Convert a domain include to an entity include
    Expression<Func<TEntity, object>> CreateEntityInclude(
        Expression<Func<TDomain?, object>> domainInclude
    );

    TDomain? ToDomain(TEntity entity);
    TEntity? ToEntity(TDomain domain);
}