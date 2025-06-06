using Domain;

namespace Core.Interfaces.Factories;

public interface IRepositoryResultFactory
{
    RepositoryResult<TValue> OperationSuccess<TValue>(TValue value, int statusCode = 200);
    string? OperationFailed(char error, int statusCode = 200);
    RepositoryResult<TValue> OperationFailed<TValue>(Error error, int statusCode);
}
