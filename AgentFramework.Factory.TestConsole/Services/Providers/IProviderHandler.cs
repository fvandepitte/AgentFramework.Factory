namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Interface for provider handlers in the chain of responsibility (extends core abstraction)
/// </summary>
public interface IProviderHandler : Factory.Abstractions.IProviderHandler
{
    /// <summary>
    /// Get the next handler in the chain (if any)
    /// </summary>
    IProviderHandler? NextHandler { get; }

    /// <summary>
    /// Determine if this provider can handle the specified model
    /// </summary>
    bool CanHandleModel(string modelName);
}
