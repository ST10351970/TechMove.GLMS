namespace TechMove.GLMS.Core.Services;

/// <summary>
/// Result types for validation operations.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, Array.Empty<string>());

    public static ValidationResult Failure(string error) => new(false, new[] { error });

    public static ValidationResult Failure(IEnumerable<string> errors)
        => new(false, errors.ToList().AsReadOnly());

    public string ErrorSummary => string.Join(" ", Errors);
}