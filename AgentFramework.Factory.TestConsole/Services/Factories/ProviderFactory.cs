using AgentFramework.Factory.TestConsole.Services.Configuration;
using AgentFramework.Factory.TestConsole.Services.Models;
using AgentFramework.Factory.TestConsole.Services.Providers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.TestConsole.Services.Factories;

/// <summary>
/// Factory for creating IChatClient instances using Chain of Responsibility pattern
/// </summary>
public class ProviderFactory
{
    private readonly AppConfiguration configuration;
    private readonly IProviderHandler providerChainHead;

    public ProviderFactory(IOptions<AppConfiguration> configOptions, IEnumerable<IProviderHandler> providerHandlers)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        this.configuration = configOptions.Value;
        
        if (providerHandlers == null || !providerHandlers.Any())
        {
            throw new ArgumentException("At least one provider handler must be registered", nameof(providerHandlers));
        }
        
        // Build the chain of responsibility from registered handlers
        providerChainHead = BuildProviderChain(providerHandlers);
    }

    /// <summary>
    /// Build the provider chain based on configuration and registered handlers
    /// </summary>
    private IProviderHandler BuildProviderChain(IEnumerable<IProviderHandler> providerHandlers)
    {
        // Create a dictionary of handlers by their provider name
        var handlersByName = providerHandlers.ToDictionary(
            h => h.ProviderName,
            h => h,
            StringComparer.OrdinalIgnoreCase);

        // Get the provider chain order from configuration
        List<string> providerOrder;
        
        if (configuration.AgentFactory.ProviderChain != null && configuration.AgentFactory.ProviderChain.Any())
        {
            // Use configured chain
            providerOrder = configuration.AgentFactory.ProviderChain;
        }
        else
        {
            // Default: start with default provider, then try others
            providerOrder = new List<string> { configuration.AgentFactory.DefaultProvider };
            
            // Add other providers that aren't the default
            foreach (var providerName in handlersByName.Keys)
            {
                if (!providerName.Equals(configuration.AgentFactory.DefaultProvider, StringComparison.OrdinalIgnoreCase))
                {
                    providerOrder.Add(providerName);
                }
            }
        }

        // Build the chain
        IProviderHandler? head = null;
        IProviderHandler? current = null;

        foreach (var providerName in providerOrder)
        {
            if (handlersByName.TryGetValue(providerName, out var handler))
            {
                if (head == null)
                {
                    head = handler;
                    current = handler;
                }
                else
                {
                    current = current!.SetNext(handler);
                }
            }
            else if (configuration.AgentFactory.EnableLogging)
            {
                Console.WriteLine($"  ⚠ Unknown provider in chain: {providerName}");
            }
        }

        if (head == null)
        {
            throw new InvalidOperationException("No valid providers configured in the chain");
        }

        return head;
    }

    /// <summary>
    /// Create an IChatClient instance for the specified model
    /// </summary>
    /// <param name="modelName">The model name to create a client for</param>
    /// <returns>IChatClient instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when no provider in the chain can handle the model</exception>
    public IChatClient CreateChatClient(string modelName)
    {
        if (string.IsNullOrEmpty(modelName))
        {
            throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));
        }

        if (configuration.AgentFactory.EnableLogging)
        {
            Console.WriteLine($"🔍 Looking for provider to handle model: {modelName}");
        }

        if (providerChainHead.TryCreateChatClient(modelName, out var client) && client != null)
        {
            return client;
        }

        throw new InvalidOperationException(
            $"No provider in the chain could handle model '{modelName}'. " +
            $"Check your provider configurations and ensure at least one provider supports this model.");
    }

    /// <summary>
    /// Create an IChatClient instance for the specified agent
    /// </summary>
    public IChatClient CreateChatClientForAgent(LoadedAgent agent)
    {
        var modelName = agent.Model ?? configuration.AgentFactory.DefaultProvider;
        return CreateChatClient(modelName);
    }

    /// <summary>
    /// Validate that at least one provider in the chain is properly configured
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateProviderChain()
    {
        // Try to create a test client to ensure at least one provider is working
        // We'll use a common model name that most providers should recognize
        var testModel = "gpt-4o-mini";
        
        if (providerChainHead.TryCreateChatClient(testModel, out _))
        {
            return (true, string.Empty);
        }

        return (false, "No provider in the chain is properly configured. Check your provider settings.");
    }
}
