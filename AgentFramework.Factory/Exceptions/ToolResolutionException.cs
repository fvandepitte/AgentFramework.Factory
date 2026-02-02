namespace AgentFramework.Factory.Exceptions;

/// <summary>
/// Exception thrown when a tool cannot be resolved
/// </summary>
public class ToolResolutionException : Exception
{
    public ToolResolutionException()
    {
    }

    public ToolResolutionException(string message) : base(message)
    {
    }

    public ToolResolutionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the name of the tool that could not be resolved
    /// </summary>
    public string? ToolName { get; init; }

    /// <summary>
    /// Gets the agent name that requested the tool
    /// </summary>
    public string? AgentName { get; init; }
}
