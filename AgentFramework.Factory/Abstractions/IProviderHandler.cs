using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Defines a provider handler for creating chat clients based on models
/// </summary>
public interface IProviderHandler
{
    /// <summary>
    /// Gets the name of this provider (e.g., "azureOpenAI", "openAI", "githubModels")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Determines if this handler can create a client for the specified model
    /// </summary>
    /// <param name="modelName">The model name to check</param>
    /// <returns>True if this handler can create a client for the model</returns>
    bool CanHandle(string modelName);

    /// <summary>
    /// Creates a chat client for the specified model
    /// </summary>
    /// <param name="modelName">The model name</param>
    /// <returns>IChatClient instance or null if unable to create</returns>
    IChatClient? CreateChatClient(string modelName);

    /// <summary>
    /// Sets the next handler in the chain of responsibility
    /// </summary>
    /// <param name="nextHandler">The next handler to try if this one cannot handle the request</param>
    /// <returns>The next handler for fluent chaining</returns>
    IProviderHandler SetNext(IProviderHandler nextHandler);

    /// <summary>
    /// Handles the request using the chain of responsibility pattern
    /// </summary>
    /// <param name="modelName">The model name</param>
    /// <param name="onSuccess">Optional callback invoked when a provider successfully creates a client</param>
    /// <param name="onFailure">Optional callback invoked when a provider fails to create a client</param>
    /// <returns>IChatClient instance or null if no handler in the chain could create a client</returns>
    IChatClient? Handle(
        string modelName,
        Action<IProviderHandler, string>? onSuccess = null,
        Action<IProviderHandler, string, Exception>? onFailure = null);
}
