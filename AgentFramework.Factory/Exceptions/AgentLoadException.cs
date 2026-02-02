namespace AgentFramework.Factory.Exceptions;

/// <summary>
/// Exception thrown when an agent cannot be loaded
/// </summary>
public class AgentLoadException : Exception
{
    public AgentLoadException()
    {
    }

    public AgentLoadException(string message) : base(message)
    {
    }

    public AgentLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the agent name that failed to load
    /// </summary>
    public string? AgentName { get; init; }

    /// <summary>
    /// Gets the path to the markdown file that failed to load
    /// </summary>
    public string? FilePath { get; init; }
}
