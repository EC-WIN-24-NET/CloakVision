using Core.Interfaces.Factories;
using Domain;

namespace Core.Factories.Data;

/// <summary>
/// Thanks, Hans, for inspiring me to do this https://www.youtube.com/watch?v=hKuZVyuS_Xc&ab_channel=EPNSverigeAB-Utbildning
/// The <c>RepositoryResultFactory</c> class is responsible for creating consistent
/// and structured results for repository operations. It provides methods to simplify
/// the creation of success and failure results for data operations, encapsulating
/// the result status, data, and any associated error information.
/// </summary>
/// <remarks>
/// This class is particularly useful when creating uniform responses for repository
/// operations, ensuring a standardized way to handle both success and failure scenarios.
/// It works closely with the <c>RepositoryResult</c> class to encapsulate operation results.
/// </remarks>
/// <example>
/// Use this factory to create instances of <c>RepositoryResult</c> for either successful
/// or failed operations. It ensures proper error handling and status code assignment.
/// </example>
public class RepositoryResultFactory : IRepositoryResultFactory
{
    // The class is sealed to prevent inheritance, ensuring that the factory's behavior
    public RepositoryResult<TValue> OperationSuccess<TValue>(TValue value, int statusCode = 200)
    {
        return new RepositoryResult<TValue>(value, statusCode);
    }

    public string? OperationFailed(char error, int statusCode = 200)
    {
        return new string(error, statusCode);
    }

    // The class is sealed to prevent inheritance, ensuring that the factory's behavior
    public RepositoryResult<TValue> OperationFailed<TValue>(Error error, int statusCode)
    {
        if (error == Error.NonError)
            throw new InvalidOperationException(
                "Cannot create a failure result with this type of error."
            );
        return new RepositoryResult<TValue>(error, statusCode);
    }
}
