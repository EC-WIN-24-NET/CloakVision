namespace Domain;

/// <summary>
/// Refactored using Google Gemini Pro to use Const for follow Dry principle
/// Represents an error with a specific error code and message.
/// </summary>
public record Error(string ErrorCode, string Message, string? FieldName = null)
{
    // Using Const,
    private const string NotFoundErrorCode = "Error.NotFound";
    private const string NullValueErrorCode = "Error.NullValue";

    // Sending response on no errors
    public static readonly Error NonError = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new(NullValueErrorCode, "The result value is null.");

    // Error Not Found
    public static Error NotFound(string entityName)
    {
        return new Error(NotFoundErrorCode, $"{entityName} not found.");
    }

    // Error Invalid Field, for UI
    public static Error InvalidField(string fieldName, string message)
    {
        return new Error("Error.InvalidField", message, fieldName);
    }
}
