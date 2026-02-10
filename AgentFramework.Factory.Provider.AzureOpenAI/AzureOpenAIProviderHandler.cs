using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.AzureOpenAI.Configuration;
using AgentFramework.Factory.Services;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ClientModel;

namespace AgentFramework.Factory.Provider.AzureOpenAI;

/// <summary>
/// Provider handler for Azure OpenAI
/// </summary>
public class AzureOpenAIProviderHandler : BaseProviderHandler
{
    private readonly AzureOpenAIConfiguration config;

    public AzureOpenAIProviderHandler(
        IOptions<AzureOpenAIConfiguration> azureConfigOptions,
        ILogger<AzureOpenAIProviderHandler> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(azureConfigOptions);
        this.config = azureConfigOptions.Value;
    }

    public override string ProviderName => "AzureOpenAI";

    public override bool CanHandle(string modelName)
    {
        // Check if Azure OpenAI is configured
        if (string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.DeploymentName))
        {
            return false;
        }

        // Azure OpenAI uses deployment names, which can be different from model names
        // For now, we'll assume it can handle any model if it's configured
        // In a real implementation, you might want to query available deployments
        return true;
    }

    public override IChatClient? CreateChatClient(string modelName)
    {
        if (string.IsNullOrEmpty(config.Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured");
        }

        if (string.IsNullOrEmpty(config.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI deployment name is not configured");
        }

        // Create Azure OpenAI client with either API key or DefaultAzureCredential
        AzureOpenAIClient azureClient;

        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            // Use API key authentication
            azureClient = new AzureOpenAIClient(
                new Uri(config.Endpoint),
                new ApiKeyCredential(config.ApiKey));
        }
        else
        {
            // Use Azure CLI / Managed Identity authentication
            azureClient = new AzureOpenAIClient(
                new Uri(config.Endpoint),
                new DefaultAzureCredential());
        }

        // Get chat client and convert to IChatClient
        IChatClient chatClient = azureClient
            .GetChatClient(config.DeploymentName)
            .AsIChatClient();

        return chatClient;
    }
}
