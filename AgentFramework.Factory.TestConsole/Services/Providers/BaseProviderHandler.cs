using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Abstract base class for provider handlers implementing the Chain of Responsibility pattern
/// </summary>
public abstract class BaseProviderHandler : IProviderHandler
{
    private IProviderHandler? _nextHandler;
    protected readonly AppConfiguration Configuration;

    protected BaseProviderHandler(IOptions<AppConfiguration> configOptions)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        Configuration = configOptions.Value;
    }

    public abstract string ProviderName { get; }

    public IProviderHandler SetNext(IProviderHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public bool TryCreateChatClient(string modelName, out IChatClient? client)
    {
        // First, try to handle the request in this provider
        if (CanHandleModel(modelName))
        {
            try
            {
                client = CreateChatClient(modelName);
                if (Configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ✓ Provider '{ProviderName}' handling model: {modelName}");
                }
                return true;
            }
            catch (Exception ex)
            {
                if (Configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ✗ Provider '{ProviderName}' failed to create client: {ex.Message}");
                }
                client = null;
            }
        }

        // If this provider can't handle it, pass to the next handler in the chain
        if (_nextHandler != null)
        {
            return _nextHandler.TryCreateChatClient(modelName, out client);
        }

        // No handler in the chain could handle the request
        client = null;
        return false;
    }

    /// <summary>
    /// Determine if this provider can handle the specified model
    /// </summary>
    protected abstract bool CanHandleModel(string modelName);

    /// <summary>
    /// Create the chat client for the specified model
    /// </summary>
    protected abstract IChatClient CreateChatClient(string modelName);
}
