using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.OpenAI.Configuration;
using AgentFramework.Factory.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ClientModel;

namespace AgentFramework.Factory.Provider.OpenAI;

/// <summary>
/// Provider handler for OpenAI
/// </summary>
public class OpenAIProviderHandler : BaseProviderHandler
{
    private readonly OpenAIConfiguration config;

    // Common OpenAI models
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "gpt-4o",
        "gpt-4o-mini",
        "gpt-4-turbo",
        "gpt-4",
        "gpt-3.5-turbo",
        "o1",
        "o1-mini",
        "o1-preview"
    };

    public OpenAIProviderHandler(
        IOptions<OpenAIConfiguration> openAIConfigOptions,
        ILogger<OpenAIProviderHandler> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(openAIConfigOptions);
        this.config = openAIConfigOptions.Value;
    }

    public override string ProviderName => "OpenAI";

    public override bool CanHandle(string modelName)
    {
        // Check if OpenAI is configured
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            return false;
        }

        // Check if the model name matches known OpenAI models
        return SupportedModels.Contains(modelName);
    }

    public override IChatClient? CreateChatClient(string modelName)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        // Create OpenAI chat client using OpenAI client
        var openAIClient = new global::OpenAI.OpenAIClient(
            new ApiKeyCredential(config.ApiKey));

        IChatClient chatClient = openAIClient
            .GetChatClient(modelName)
            .AsIChatClient();

        return chatClient;
    }
}
