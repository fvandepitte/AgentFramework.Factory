using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.ClientModel;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Provider handler for GitHub Models
/// </summary>
public class GitHubModelsProviderHandler : BaseProviderHandler
{
    private readonly GitHubModelsConfiguration _config;

    // GitHub Models supports various models through AI marketplace
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "gpt-4o",
        "gpt-4o-mini",
        "gpt-4-turbo",
        "gpt-4",
        "gpt-3.5-turbo",
        "o1",
        "o1-mini",
        "o1-preview",
        "phi-3",
        "phi-3.5",
        "llama-3",
        "llama-3.1",
        "llama-3.2",
        "mistral-large",
        "mistral-nemo",
        "cohere-command-r",
        "cohere-command-r-plus"
    };

    public GitHubModelsProviderHandler(IOptions<GitHubModelsConfiguration> githubConfigOptions)
    {
        ArgumentNullException.ThrowIfNull(githubConfigOptions);
        _config = githubConfigOptions.Value;
    }

    public override string ProviderName => "GitHubModels";

    public override bool CanHandleModel(string modelName)
    {
        // Check if GitHub Models is configured
        if (string.IsNullOrEmpty(_config.Token))
        {
            return false;
        }

        // Check if the model name matches known GitHub Models
        return SupportedModels.Contains(modelName);
    }

    public override IChatClient CreateChatClient(string modelName)
    {
        if (string.IsNullOrEmpty(_config.Token))
        {
            throw new InvalidOperationException("GitHub token is not configured");
        }

        // Create GitHub Models chat client using OpenAI-compatible endpoint
        var openAIClient = new OpenAI.OpenAIClient(
            new ApiKeyCredential(_config.Token),
            new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri("https://models.inference.ai.azure.com")
            });

        IChatClient chatClient = openAIClient
            .GetChatClient(modelName)
            .AsIChatClient();

        return chatClient;
    }
}
