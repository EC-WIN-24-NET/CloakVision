namespace Domain;

/// <summary>
/// Inspired by Hans, you are awesome, thanks :)
/// </summary>
public class RepositoryResult<TValue>
{
    public TValue? Value { get; }
    public Error Error { get; }
    public int StatusCode { get; }

    /// <summary>
    /// Internal constructor for creating a success result.
    /// Intended only for use by the factory.
    /// </summary>
    public RepositoryResult(TValue value, int statusCode = 200)
    {
        Value = value;
        StatusCode = statusCode;
        Error = Error.NonError;
    }

    /// <summary>
    /// Internal constructor for creating a failure result.
    /// Intended only for use by the factory. Assume the factory does error validation.
    /// </summary>
    public RepositoryResult(Error error, int statusCode)
    {
        // Basic validation: Don't allow success codes for errors, default to 400 or 500 if needed
        // Or throw an exception, depending on desired strictness
        StatusCode = statusCode is >= 200 and < 300 ? 400 : statusCode;
        Error = error ?? throw new ArgumentNullException(nameof(error));
        Value = default;
    }
}
