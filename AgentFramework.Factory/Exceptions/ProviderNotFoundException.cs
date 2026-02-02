namespace AgentFramework.Factory.Exceptions;

/// <summary>
/// Exception thrown when a requested provider cannot be found
/// </summary>
public class ProviderNotFoundException : Exception
{
    public ProviderNotFoundException()
    {
    }

    public ProviderNotFoundException(string message) : base(message)
    {
    }

    public ProviderNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the name of the provider that was not found
    /// </summary>
    public string? ProviderName { get; init; }
}
