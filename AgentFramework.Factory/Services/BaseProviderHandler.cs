using AgentFramework.Factory.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgentFramework.Factory.Services;

/// <summary>
/// Base implementation of IProviderHandler using Chain of Responsibility pattern
/// </summary>
public abstract class BaseProviderHandler : IProviderHandler
{
    protected readonly ILogger logger;
    private IProviderHandler? nextHandler;

    protected BaseProviderHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public abstract string ProviderName { get; }

    public abstract bool CanHandle(string modelName);

    public abstract IChatClient? CreateChatClient(string modelName);

    public IProviderHandler SetNext(IProviderHandler nextHandler)
    {
        this.nextHandler = nextHandler;
        return nextHandler;
    }

    public IChatClient? Handle(
        string modelName,
        Action<IProviderHandler, string>? onSuccess = null,
        Action<IProviderHandler, string, Exception>? onFailure = null)
    {
        if (CanHandle(modelName))
        {
            try
            {
                var client = CreateChatClient(modelName);
                if (client != null)
                {
                    logger.LogInformation("Provider {ProviderName} successfully created client for model {ModelName}",
                        ProviderName, modelName);
                    onSuccess?.Invoke(this, modelName);
                    return client;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Provider {ProviderName} failed to create client for model {ModelName}",
                    ProviderName, modelName);
                onFailure?.Invoke(this, modelName, ex);
            }
        }

        // Try the next handler in the chain
        if (nextHandler != null)
        {
            logger.LogDebug("Provider {ProviderName} cannot handle model {ModelName}, trying next handler",
                ProviderName, modelName);
            return nextHandler.Handle(modelName, onSuccess, onFailure);
        }

        logger.LogError("No provider in the chain could handle model {ModelName}", modelName);
        return null;
    }
}
