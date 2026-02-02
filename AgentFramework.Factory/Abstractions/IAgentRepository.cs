namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Defines a repository for loading and saving agents from various sources
/// </summary>
public interface IAgentRepository
{
    /// <summary>
    /// Loads an agent by name
    /// </summary>
    /// <param name="name">The agent name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The loaded agent or null if not found</returns>
    Task<ILoadedAgent?> LoadAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all agents from the repository
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of loaded agents</returns>
    Task<IEnumerable<ILoadedAgent>> LoadAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an agent to the repository
    /// </summary>
    /// <param name="agent">The agent to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAsync(ILoadedAgent agent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an agent from the repository
    /// </summary>
    /// <param name="name">The agent name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an agent exists in the repository
    /// </summary>
    /// <param name="name">The agent name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the agent exists</returns>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
