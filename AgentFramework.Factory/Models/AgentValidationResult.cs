namespace AgentFramework.Factory.Models;

/// <summary>
/// Structured validation result for markdown parsing
/// </summary>
public class AgentValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of validation errors
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of validation warnings
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static AgentValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static AgentValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors };

    /// <summary>
    /// Creates a validation result with warnings
    /// </summary>
    public static AgentValidationResult WithWarnings(params string[] warnings) =>
        new() { IsValid = true, Warnings = warnings };
}
