namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Defines an agent runner that can execute conversations with loaded agents
/// </summary>
public interface IAgentRunner
{
    /// <summary>
    /// Executes a conversation with the specified agent
    /// </summary>
    /// <param name="agent">The loaded agent to converse with</param>
    /// <param name="userMessage">The user's message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent's response</returns>
    Task<string> RunAsync(ILoadedAgent agent, string userMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a streaming conversation with the specified agent
    /// </summary>
    /// <param name="agent">The loaded agent to converse with</param>
    /// <param name="userMessage">The user's message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of response chunks</returns>
    IAsyncEnumerable<string> RunStreamingAsync(ILoadedAgent agent, string userMessage, CancellationToken cancellationToken = default);
}
