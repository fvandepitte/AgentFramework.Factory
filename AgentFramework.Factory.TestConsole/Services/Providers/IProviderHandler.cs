using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Interface for provider handlers in the chain of responsibility
/// </summary>
public interface IProviderHandler
{
    /// <summary>
    /// Get the provider name
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Get the next handler in the chain (if any)
    /// </summary>
    IProviderHandler? NextHandler { get; }

    /// <summary>
    /// Set the next handler in the chain
    /// </summary>
    IProviderHandler SetNext(IProviderHandler handler);

    /// <summary>
    /// Determine if this provider can handle the specified model
    /// </summary>
    bool CanHandleModel(string modelName);

    /// <summary>
    /// Create a chat client for the specified model
    /// </summary>
    IChatClient CreateChatClient(string modelName);

    /// <summary>
    /// Handle the model request, optionally with callbacks for logging/monitoring
    /// </summary>
    IChatClient? Handle(
        string modelName, 
        Action<IProviderHandler, string>? onSuccess = null, 
        Action<IProviderHandler, string, Exception>? onFailure = null);
}
