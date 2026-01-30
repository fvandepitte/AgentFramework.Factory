using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.ClientModel;

namespace AgentFramework.Factory.TestConsole.Services.Providers;

/// <summary>
/// Provider handler for OpenAI
/// </summary>
public class OpenAIProviderHandler : BaseProviderHandler
{
    private readonly OpenAIConfiguration _config;

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

    public OpenAIProviderHandler(IOptions<OpenAIConfiguration> openAIConfigOptions)
    {
        ArgumentNullException.ThrowIfNull(openAIConfigOptions);
        _config = openAIConfigOptions.Value;
    }

    public override string ProviderName => "OpenAI";

    public override bool CanHandleModel(string modelName)
    {
        // Check if OpenAI is configured
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return false;
        }

        // Check if the model name matches known OpenAI models
        return SupportedModels.Contains(modelName);
    }

    public override IChatClient CreateChatClient(string modelName)
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        // Create OpenAI chat client using OpenAI client
        var openAIClient = new OpenAI.OpenAIClient(
            new ApiKeyCredential(_config.ApiKey));

        IChatClient chatClient = openAIClient
            .GetChatClient(modelName)
            .AsIChatClient();

        return chatClient;
    }
}
