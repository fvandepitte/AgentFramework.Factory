using AgentFramework.Factory.TestConsole.Services.Configuration;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.ClientModel;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Provider handler for Azure OpenAI
/// </summary>
public class AzureOpenAIProviderHandler : BaseProviderHandler
{
    private readonly AzureOpenAIConfiguration _config;

    public AzureOpenAIProviderHandler(IOptions<AzureOpenAIConfiguration> azureConfigOptions)
    {
        ArgumentNullException.ThrowIfNull(azureConfigOptions);
        _config = azureConfigOptions.Value;
    }

    public override string ProviderName => "AzureOpenAI";

    public override bool CanHandleModel(string modelName)
    {
        // Check if Azure OpenAI is configured
        if (string.IsNullOrEmpty(_config.Endpoint) || string.IsNullOrEmpty(_config.DeploymentName))
        {
            return false;
        }

        // Azure OpenAI uses deployment names, which can be different from model names
        // For now, we'll assume it can handle any model if it's configured
        // In a real implementation, you might want to query available deployments
        return true;
    }

    public override IChatClient CreateChatClient(string modelName)
    {
        if (string.IsNullOrEmpty(_config.Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured");
        }

        if (string.IsNullOrEmpty(_config.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI deployment name is not configured");
        }

        // Create Azure OpenAI client with either API key or DefaultAzureCredential
        AzureOpenAIClient azureClient;

        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            // Use API key authentication
            azureClient = new AzureOpenAIClient(
                new Uri(_config.Endpoint),
                new ApiKeyCredential(_config.ApiKey));
        }
        else
        {
            // Use Azure CLI / Managed Identity authentication
            azureClient = new AzureOpenAIClient(
                new Uri(_config.Endpoint),
                new DefaultAzureCredential());
        }

        // Get chat client and convert to IChatClient
        IChatClient chatClient = azureClient
            .GetChatClient(_config.DeploymentName)
            .AsIChatClient();

        return chatClient;
    }
}
