using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Abstract base class for provider handlers implementing the Chain of Responsibility pattern
/// </summary>
public abstract class BaseProviderHandler : IProviderHandler
{
    private IProviderHandler? nextHandler;

    public abstract string ProviderName { get; }

    public IProviderHandler? NextHandler => nextHandler;

    public IProviderHandler SetNext(IProviderHandler handler)
    {
        nextHandler = handler;
        return handler;
    }

    Factory.Abstractions.IProviderHandler Factory.Abstractions.IProviderHandler.SetNext(Factory.Abstractions.IProviderHandler nextHandler)
    {
        return SetNext((IProviderHandler)nextHandler);
    }

    public IChatClient? Handle(
        string modelName, 
        Action<IProviderHandler, string>? onSuccess = null, 
        Action<IProviderHandler, string, Exception>? onFailure = null)
    {
        if (!CanHandleModel(modelName))
        {
            // If there's a next handler, call it through the base interface to avoid type conflicts
            if (NextHandler != null)
            {
                return ((Factory.Abstractions.IProviderHandler)NextHandler).Handle(
                    modelName,
                    onSuccess != null ? (h, m) => onSuccess((IProviderHandler)h, m) : null,
                    onFailure != null ? (h, m, ex) => onFailure((IProviderHandler)h, m, ex) : null);
            }
            return null;
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
            // If there's a next handler, call it through the base interface to avoid type conflicts
            if (NextHandler != null)
            {
                return ((Factory.Abstractions.IProviderHandler)NextHandler).Handle(
                    modelName,
                    onSuccess != null ? (h, m) => onSuccess((IProviderHandler)h, m) : null,
                    onFailure != null ? (h, m, ex) => onFailure((IProviderHandler)h, m, ex) : null);
            }
            return null;
        }
    }

    IChatClient? Factory.Abstractions.IProviderHandler.Handle(
        string modelName,
        Action<Factory.Abstractions.IProviderHandler, string>? onSuccess,
        Action<Factory.Abstractions.IProviderHandler, string, Exception>? onFailure)
    {
        // Convert the callbacks to use the derived interface type
        Action<IProviderHandler, string>? wrappedSuccess = null;
        Action<IProviderHandler, string, Exception>? wrappedFailure = null;

        if (onSuccess != null)
        {
            wrappedSuccess = (h, m) => onSuccess(h, m);
        }

        if (onFailure != null)
        {
            wrappedFailure = (h, m, ex) => onFailure(h, m, ex);
        }

        return Handle(modelName, wrappedSuccess, wrappedFailure);
    }

    public abstract bool CanHandleModel(string modelName);

    public bool CanHandle(string modelName) => CanHandleModel(modelName);

    public abstract IChatClient CreateChatClient(string modelName);

    IChatClient? Factory.Abstractions.IProviderHandler.CreateChatClient(string modelName)
    {
        return CreateChatClient(modelName);
    }
}
