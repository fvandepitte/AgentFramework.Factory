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

    public OpenAIProviderHandler(IOptions<AppConfiguration> configOptions) : base(configOptions)
    {
    }

    public override string ProviderName => "OpenAI";

    protected override bool CanHandleModel(string modelName)
    {
        var config = Configuration.Providers.OpenAI;

        // Check if OpenAI is configured
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            return false;
        }

        // Check if the model name matches known OpenAI models
        return SupportedModels.Contains(modelName);
    }

    protected override IChatClient CreateChatClient(string modelName)
    {
        var config = Configuration.Providers.OpenAI;

        if (string.IsNullOrEmpty(config.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        // Create OpenAI chat client using OpenAI client
        var openAIClient = new OpenAI.OpenAIClient(
            new ApiKeyCredential(config.ApiKey));

        IChatClient chatClient = openAIClient
            .GetChatClient(modelName)
            .AsIChatClient();

        return chatClient;
    }
}
