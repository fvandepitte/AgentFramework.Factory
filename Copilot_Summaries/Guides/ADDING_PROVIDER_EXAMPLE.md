# Adding a New Provider Handler - Example

This guide shows how easy it is to add a new provider handler with the DI setup.

## Example: Adding Anthropic Claude Support

### Step 1: Create the Provider Handler

Create `AnthropicProviderHandler.cs` in the `Services/` folder:

```csharp
using Microsoft.Extensions.AI;
using System.ClientModel;

namespace AgentFramework.Factory.TestConsole.Services;

/// <summary>
/// Provider handler for Anthropic Claude
/// </summary>
public class AnthropicProviderHandler : BaseProviderHandler
{
    // Known Anthropic models
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "claude-3-opus",
        "claude-3-sonnet",
        "claude-3-haiku",
        "claude-3.5-sonnet",
        "claude-3.5-haiku"
    };

    public AnthropicProviderHandler(AppConfiguration configuration) : base(configuration)
    {
    }

    public override string ProviderName => "Anthropic";

    protected override bool CanHandleModel(string modelName)
    {
        var config = Configuration.Providers.Anthropic;

        // Check if Anthropic is configured
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            return false;
        }

        // Check if the model name matches known Anthropic models
        return SupportedModels.Contains(modelName);
    }

    protected override IChatClient CreateChatClient(string modelName)
    {
        var config = Configuration.Providers.Anthropic;

        if (string.IsNullOrEmpty(config.ApiKey))
        {
            throw new InvalidOperationException("Anthropic API key is not configured");
        }

        // Create Anthropic chat client
        // (This is pseudocode - actual implementation depends on Anthropic SDK)
        var anthropicClient = new Anthropic.AnthropicClient(
            new ApiKeyCredential(config.ApiKey));

        IChatClient chatClient = anthropicClient
            .GetChatClient(modelName)
            .AsIChatClient();

        return chatClient;
    }
}
```

### Step 2: Add Configuration Model

Add to `Configuration.cs`:

```csharp
public class ProvidersConfiguration
{
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();
    public OpenAIConfiguration OpenAI { get; set; } = new();
    public GitHubModelsConfiguration GitHubModels { get; set; } = new();
    public AnthropicConfiguration Anthropic { get; set; } = new();  // New!
}

public class AnthropicConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-3.5-sonnet";
}
```

### Step 3: Register in DI Container

Update `ServiceCollectionExtensions.cs`:

```csharp
// Register provider handlers
services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();
services.AddSingleton<IProviderHandler, AnthropicProviderHandler>();  // New!
```

### Step 4: Configure in appsettings.json

Add configuration:

```json
{
  "agentFactory": {
    "providerChain": ["azureOpenAI", "openAI", "anthropic", "githubModels"]
  },
  "providers": {
    "anthropic": {
      "apiKey": "",
      "model": "claude-3.5-sonnet"
    }
  }
}
```

### Step 5: Set API Key (User Secrets)

```bash
dotnet user-secrets set "providers:anthropic:apiKey" "your-api-key"
```

## That's It! 🎉

The new provider is now:
- ✅ Automatically injected into `ProviderFactory`
- ✅ Part of the provider chain
- ✅ Available for all agents
- ✅ Fully integrated with the Chain of Responsibility pattern

## Testing the New Provider

### Test with a Claude Model

Create an agent that uses Claude:

```markdown
---
name: ClaudeAssistant
description: AI assistant powered by Claude
model: claude-3.5-sonnet
temperature: 0.7
---

# Persona
You are a helpful AI assistant powered by Anthropic's Claude.
```

Run the application:

```bash
dotnet run -- show ClaudeAssistant
```

### Verify Chain Behavior

The chain will automatically try providers in order:
1. Azure OpenAI (if configured and model supported)
2. OpenAI (if model is an OpenAI model)
3. **Anthropic** (if model starts with "claude-")
4. GitHub Models (fallback)

## Benefits of DI-Based Approach

### Before (Manual Instantiation)
```csharp
// ProviderFactory had to know about all providers
private IProviderHandler BuildProviderChain()
{
    var providers = new Dictionary<string, Func<IProviderHandler>>
    {
        ["azureopenai"] = () => new AzureOpenAIProviderHandler(config),
        ["openai"] = () => new OpenAIProviderHandler(config),
        ["githubmodels"] = () => new GitHubModelsProviderHandler(config),
        ["anthropic"] = () => new AnthropicProviderHandler(config)  // Have to add here!
    };
    // ...
}
```

❌ Have to modify `ProviderFactory` to add a provider  
❌ Tight coupling between factory and concrete handlers  
❌ Hard to test (can't easily mock handlers)  

### After (DI-Based)
```csharp
// ProviderFactory receives all registered handlers
public ProviderFactory(
    AppConfiguration configuration,
    IEnumerable<IProviderHandler> providerHandlers)
{
    // Just use whatever handlers were registered
    providerChainHead = BuildProviderChain(providerHandlers);
}
```

✅ Zero changes to `ProviderFactory` when adding providers  
✅ Loose coupling via dependency injection  
✅ Easy to test (inject mock handlers)  
✅ Open/Closed Principle - open for extension, closed for modification  

## Adding Multiple Providers at Once

You can add multiple providers in one go:

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<IProviderHandler, AnthropicProviderHandler>();
services.AddSingleton<IProviderHandler, CohereProviderHandler>();
services.AddSingleton<IProviderHandler, HuggingFaceProviderHandler>();
services.AddSingleton<IProviderHandler, MistralProviderHandler>();
```

All will be automatically injected into `ProviderFactory` and available in the chain!

---

**Pattern**: Dependency Injection + Strategy Pattern + Chain of Responsibility  
**Principle**: Open/Closed Principle (SOLID)  
**Result**: Extensible, testable, maintainable code ✨

