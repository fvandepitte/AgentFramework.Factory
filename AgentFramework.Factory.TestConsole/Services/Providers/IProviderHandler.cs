using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Interface for provider handlers in the chain of responsibility
/// </summary>
public interface IProviderHandler
{
    /// <summary>
    /// Set the next handler in the chain
    /// </summary>
    IProviderHandler SetNext(IProviderHandler handler);

    /// <summary>
    /// Try to create a chat client for the specified model
    /// </summary>
    /// <param name="modelName">The model name to create a client for</param>
    /// <param name="client">The created chat client if successful</param>
    /// <returns>True if this provider can handle the model, false otherwise</returns>
    bool TryCreateChatClient(string modelName, out IChatClient? client);

    /// <summary>
    /// Get the provider name
    /// </summary>
    string ProviderName { get; }
}
