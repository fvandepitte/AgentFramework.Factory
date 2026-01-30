using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;

namespace AgentFramework.Factory.TestConsole.Services;

/// <summary>
/// Factory for creating IChatClient instances from configuration
/// </summary>
public class ProviderFactory
{
    private readonly AppConfiguration configuration;

    public ProviderFactory(AppConfiguration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Create an IChatClient instance for the specified provider
    /// </summary>
    public IChatClient CreateChatClient(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "azureopenai" => CreateAzureOpenAIChatClient(),
            _ => throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName))
        };
    }

    /// <summary>
    /// Create an IChatClient instance for the specified agent
    /// </summary>
    public IChatClient CreateChatClientForAgent(LoadedAgent agent)
    {
        var providerName = agent.Provider ?? configuration.AgentFactory.DefaultProvider;
        return CreateChatClient(providerName);
    }

    private IChatClient CreateAzureOpenAIChatClient()
    {
        var config = configuration.Providers.AzureOpenAI;

        if (string.IsNullOrEmpty(config.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured. " +
                "Please set it in appsettings.json or via environment variable AZURE_OPENAI_ENDPOINT");
        }

        if (string.IsNullOrEmpty(config.DeploymentName))
        {
            throw new InvalidOperationException(
                "Azure OpenAI deployment name is not configured. " +
                "Please set it in appsettings.json or via environment variable AZURE_OPENAI_DEPLOYMENT_NAME");
        }

        try
        {
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
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create Azure OpenAI chat client: {ex.Message}", ex);
        }
    }

    private IChatClient CreateOpenAIChatClient()
    {
        var config = configuration.Providers.OpenAI;

        if (string.IsNullOrEmpty(config.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. " +
                "Please set it in appsettings.json, user secrets, or via environment variable OPENAI_API_KEY");
        }

        if (string.IsNullOrEmpty(config.Model))
        {
            throw new InvalidOperationException(
                "OpenAI model is not configured. " +
                "Please set it in appsettings.json or via environment variable OPENAI_MODEL");
        }

        try
        {
            // Create OpenAI chat client using OpenAI client
            var openAIClient = new OpenAI.OpenAIClient(
                new ApiKeyCredential(config.ApiKey));

            IChatClient chatClient = openAIClient
                .GetChatClient(config.Model)
                .AsIChatClient();

            return chatClient;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create OpenAI chat client: {ex.Message}", ex);
        }
    }

    private IChatClient CreateGitHubModelsChatClient()
    {
        var config = configuration.Providers.GitHubModels;

        if (string.IsNullOrEmpty(config.Token))
        {
            throw new InvalidOperationException(
                "GitHub token is not configured. " +
                "Please set it in appsettings.json, user secrets, or via environment variable GITHUB_TOKEN");
        }

        if (string.IsNullOrEmpty(config.Model))
        {
            throw new InvalidOperationException(
                "GitHub model is not configured. " +
                "Please set it in appsettings.json or via environment variable GITHUB_MODEL");
        }

        try
        {
            // Create GitHub Models chat client using OpenAI-compatible endpoint
            var openAIClient = new OpenAI.OpenAIClient(
                new ApiKeyCredential(config.Token),
                new OpenAI.OpenAIClientOptions
                {
                    Endpoint = new Uri("https://models.inference.ai.azure.com")
                });

            IChatClient chatClient = openAIClient
                .GetChatClient(config.Model)
                .AsIChatClient();

            return chatClient;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create GitHub Models chat client: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validate that the specified provider is properly configured
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateProvider(string providerName)
    {
        try
        {
            return providerName.ToLowerInvariant() switch
            {
                "azureopenai" => ValidateAzureOpenAI(),
                "openai" => ValidateOpenAI(),
                "githubmodels" => ValidateGitHubModels(),
                _ => (false, $"Unknown provider: {providerName}")
            };
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private (bool IsValid, string ErrorMessage) ValidateAzureOpenAI()
    {
        var config = configuration.Providers.AzureOpenAI;

        if (string.IsNullOrEmpty(config.Endpoint))
            return (false, "Azure OpenAI endpoint is not configured");

        if (string.IsNullOrEmpty(config.DeploymentName))
            return (false, "Azure OpenAI deployment name is not configured");

        // API key is optional if using DefaultAzureCredential
        return (true, string.Empty);
    }

    private (bool IsValid, string ErrorMessage) ValidateOpenAI()
    {
        var config = configuration.Providers.OpenAI;

        if (string.IsNullOrEmpty(config.ApiKey))
            return (false, "OpenAI API key is not configured");

        if (string.IsNullOrEmpty(config.Model))
            return (false, "OpenAI model is not configured");

        return (true, string.Empty);
    }

    private (bool IsValid, string ErrorMessage) ValidateGitHubModels()
    {
        var config = configuration.Providers.GitHubModels;

        if (string.IsNullOrEmpty(config.Token))
            return (false, "GitHub token is not configured");

        if (string.IsNullOrEmpty(config.Model))
            return (false, "GitHub model is not configured");

        return (true, string.Empty);
    }
}
