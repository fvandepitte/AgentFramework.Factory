using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Abstract base class for provider handlers implementing the Chain of Responsibility pattern
/// </summary>
public abstract class BaseProviderHandler : IProviderHandler
{
    private IProviderHandler? _nextHandler;

    public abstract string ProviderName { get; }

    public IProviderHandler? NextHandler => _nextHandler;

    public IProviderHandler SetNext(IProviderHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public IChatClient? Handle(
        string modelName, 
        Action<IProviderHandler, string>? onSuccess = null, 
        Action<IProviderHandler, string, Exception>? onFailure = null)
    {
        if (!CanHandleModel(modelName))
        {
            return NextHandler?.Handle(modelName, onSuccess, onFailure);
        }

        try
        {
            var client = CreateChatClient(modelName);
            onSuccess?.Invoke(this, modelName);
            return client;
        }
        catch (Exception ex)
        {
            onFailure?.Invoke(this, modelName, ex);
            return NextHandler?.Handle(modelName, onSuccess, onFailure);
        }
    }

    public abstract bool CanHandleModel(string modelName);

    public abstract IChatClient CreateChatClient(string modelName);
}
