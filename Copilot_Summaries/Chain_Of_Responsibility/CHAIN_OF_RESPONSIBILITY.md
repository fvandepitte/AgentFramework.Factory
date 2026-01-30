# Provider Chain of Responsibility Pattern

The Agent Framework Markdown Factory now implements the **Chain of Responsibility** design pattern for provider selection. This allows automatic fallback when a provider cannot handle a requested model.

## How It Works

When you request a chat client for a specific model, the system:

1. **Starts with the first provider** in the chain (default provider or first in configured chain)
2. **Checks if that provider can handle the model** (based on model name and provider configuration)
3. **If yes**: Creates and returns the chat client
4. **If no**: Passes the request to the next provider in the chain
5. **Repeats** until a provider successfully handles the model or the chain ends

## Configuration

### Option 1: Explicit Provider Chain

Define the exact order of providers to try in `appsettings.json`:

```json
{
  "agentFactory": {
    "defaultProvider": "azureOpenAI",
    "providerChain": ["azureOpenAI", "openAI", "githubModels"],
    "enableLogging": true
  }
}
```

### Option 2: Default Behavior

If `providerChain` is empty or not specified, the system automatically builds a chain:
- **First**: The `defaultProvider`
- **Then**: All other configured providers in alphabetical order

```json
{
  "agentFactory": {
    "defaultProvider": "azureOpenAI",
    "enableLogging": true
  }
}
```

This creates the chain: `azureOpenAI` → `githubModels` → `openAI`

## Model Routing Logic

Each provider handler determines if it can handle a model based on:

### Azure OpenAI
- **Configuration check**: Endpoint and deployment name must be set
- **Model support**: Accepts any model if configured (uses deployment name, not model name)
- **Example**: If configured, will handle all models

### OpenAI
- **Configuration check**: API key must be set
- **Model support**: Known OpenAI models (`gpt-4o`, `gpt-4o-mini`, `gpt-4-turbo`, `gpt-3.5-turbo`, `o1`, etc.)
- **Example**: `gpt-4o-mini` → ✅ Can handle

### GitHub Models
- **Configuration check**: GitHub token must be set
- **Model support**: OpenAI models + additional models (`phi-3`, `llama-3`, `mistral-large`, etc.)
- **Example**: `llama-3.2` → ✅ Can handle

## Example Scenarios

### Scenario 1: Azure First, Fallback to OpenAI

**Configuration:**
```json
{
  "agentFactory": {
    "providerChain": ["azureOpenAI", "openAI"]
  },
  "providers": {
    "azureOpenAI": {
      "endpoint": "https://my-resource.openai.azure.com",
      "deploymentName": "gpt-4"
    },
    "openAI": {
      "apiKey": "sk-...",
      "model": "gpt-4o-mini"
    }
  }
}
```

**Request:** `CreateChatClient("gpt-4o-mini")`

**Flow:**
1. 🔍 Azure OpenAI: Configured ✅ → Creates client using `gpt-4` deployment
2. ✅ Returns Azure OpenAI client

---

### Scenario 2: Azure Unavailable, Fallback to OpenAI

**Configuration:**
```json
{
  "agentFactory": {
    "providerChain": ["azureOpenAI", "openAI"]
  },
  "providers": {
    "azureOpenAI": {
      "endpoint": "",  // Not configured
      "deploymentName": ""
    },
    "openAI": {
      "apiKey": "sk-...",
      "model": "gpt-4o-mini"
    }
  }
}
```

**Request:** `CreateChatClient("gpt-4o-mini")`

**Flow:**
1. 🔍 Azure OpenAI: Not configured ❌
2. 🔍 OpenAI: Configured ✅ + Model recognized ✅ → Creates OpenAI client
3. ✅ Returns OpenAI client

---

### Scenario 3: Open-Source Model via GitHub

**Configuration:**
```json
{
  "agentFactory": {
    "providerChain": ["azureOpenAI", "openAI", "githubModels"]
  },
  "providers": {
    "githubModels": {
      "token": "ghp_...",
      "model": "llama-3.2"
    }
  }
}
```

**Request:** `CreateChatClient("llama-3.2")`

**Flow:**
1. 🔍 Azure OpenAI: Model not recognized for Azure deployment ❌
2. 🔍 OpenAI: Model not in OpenAI catalog ❌
3. 🔍 GitHub Models: Configured ✅ + Model recognized ✅ → Creates GitHub client
4. ✅ Returns GitHub Models client

---

### Scenario 4: No Provider Can Handle

**Request:** `CreateChatClient("unsupported-model-xyz")`

**Flow:**
1. 🔍 Azure OpenAI: Can't verify model ❌
2. 🔍 OpenAI: Model not recognized ❌
3. 🔍 GitHub Models: Model not recognized ❌
4. ❌ Throws `InvalidOperationException`

**Error Message:**
```
No provider in the chain could handle model 'unsupported-model-xyz'. 
Check your provider configurations and ensure at least one provider supports this model.
```

## Logging Output

With `enableLogging: true`, you'll see provider chain activity:

```
🔍 Looking for provider to handle model: gpt-4o-mini
  ✓ Provider 'AzureOpenAI' handling model: gpt-4o-mini
```

Or when falling back:

```
🔍 Looking for provider to handle model: llama-3.2
  ✗ Provider 'AzureOpenAI' failed to create client: Model not configured
  ✗ Provider 'OpenAI' cannot handle model: llama-3.2
  ✓ Provider 'GitHubModels' handling model: llama-3.2
```

## Architecture

### Class Diagram

```
┌─────────────────────────┐
│  IProviderHandler       │
│  (Interface)            │
├─────────────────────────┤
│ + SetNext()             │
│ + TryCreateChatClient() │
│ + ProviderName          │
└─────────────────────────┘
            △
            │
            │ implements
            │
┌─────────────────────────┐
│  BaseProviderHandler    │
│  (Abstract)             │
├─────────────────────────┤
│ - _nextHandler          │
│ + TryCreateChatClient() │──► Calls CanHandleModel()
│ # CanHandleModel()      │    Then CreateChatClient()
│ # CreateChatClient()    │    Or passes to next handler
└─────────────────────────┘
            △
            │
            │ extends
  ┌─────────┴──────────┬───────────────────┐
  │                    │                   │
┌──────────────┐ ┌─────────────┐ ┌──────────────────┐
│AzureOpenAI   │ │  OpenAI     │ │  GitHubModels    │
│ProviderHandler│ │ProviderHandler│ │  ProviderHandler │
└──────────────┘ └─────────────┘ └──────────────────┘
```

### Sequence Diagram

```
User              ProviderFactory     Chain: Azure → OpenAI → GitHub
 │                      │                    │          │          │
 ├──CreateChatClient──►│                    │          │          │
 │   ("llama-3.2")     │                    │          │          │
 │                     ├─TryCreateClient───►│          │          │
 │                     │                    │          │          │
 │                     │                CanHandle?     │          │
 │                     │                    │──No──────┤          │
 │                     │                    │          │          │
 │                     │                    │     CanHandle?      │
 │                     │                    │          │──No──────┤
 │                     │                    │          │          │
 │                     │                    │          │     CanHandle?
 │                     │                    │          │          │──Yes─┐
 │                     │                    │          │          │      │
 │                     │                    │          │          │ CreateClient
 │                     │                    │          │          │      │
 │                     │◄──IChatClient──────┴──────────┴──────────┴──────┘
 │◄─────IChatClient────┤
 │                     │
```

## Benefits

✅ **Automatic Fallback**: No need to manually handle provider failures
✅ **Flexible Configuration**: Easy to reorder providers or add new ones
✅ **Graceful Degradation**: If primary provider is down, secondary providers take over
✅ **Model-Specific Routing**: Each provider knows which models it supports
✅ **Extensible**: Add new providers by creating a new handler class
✅ **Testable**: Each handler is independently testable

## Adding a New Provider

1. **Create a handler class** implementing `BaseProviderHandler`:

```csharp
public class MyCustomProviderHandler : BaseProviderHandler
{
    public override string ProviderName => "MyCustom";

    protected override bool CanHandleModel(string modelName)
    {
        // Check if configured and model is supported
        return !string.IsNullOrEmpty(Configuration.Providers.MyCustom.ApiKey) 
            && SupportedModels.Contains(modelName);
    }

    protected override IChatClient CreateChatClient(string modelName)
    {
        // Create and return the chat client
    }
}
```

2. **Add configuration class** to `Configuration.cs`:

```csharp
public class MyCustomConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}
```

3. **Register in ProviderFactory** `BuildProviderChain()` method:

```csharp
var providers = new Dictionary<string, Func<IProviderHandler>>
{
    // ... existing providers ...
    ["mycustom"] = () => new MyCustomProviderHandler(configuration)
};
```

4. **Add to configuration chain**:

```json
{
  "agentFactory": {
    "providerChain": ["azureOpenAI", "openAI", "mycustom", "githubModels"]
  }
}
```

## Best Practices

1. **Order providers by priority**: Put the most reliable/cost-effective provider first
2. **Enable logging during development**: See which provider is handling each request
3. **Configure fallbacks**: Always have at least 2 providers configured
4. **Model-specific chains**: Different agents can use different models, routing to different providers
5. **Monitor provider usage**: Track which providers are being used most often

---

**Last Updated**: 2026-01-30  
**Pattern**: Chain of Responsibility  
**Status**: ✅ Implemented

