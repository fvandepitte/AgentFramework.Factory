# Chain of Responsibility Example

This example demonstrates how the provider chain automatically falls back when a provider cannot handle a requested model.

## Scenario

You have three providers configured:
1. **Azure OpenAI** - Primary provider with `gpt-4` deployment
2. **OpenAI** - Fallback for standard OpenAI models
3. **GitHub Models** - Fallback for open-source models

## Configuration

```json
{
  "agentFactory": {
    "defaultProvider": "azureOpenAI",
    "providerChain": ["azureOpenAI", "openAI", "githubModels"],
    "enableLogging": true
  },
  "providers": {
    "azureOpenAI": {
      "endpoint": "https://my-resource.openai.azure.com",
      "deploymentName": "gpt-4",
      "apiVersion": "2024-08-01-preview"
    },
    "openAI": {
      "apiKey": "sk-...",
      "model": "gpt-4o-mini"
    },
    "githubModels": {
      "token": "ghp_...",
      "model": "llama-3.2"
    }
  }
}
```

## Test Cases

### Test 1: OpenAI Model → Uses OpenAI Provider

```csharp
var factory = new ProviderFactory(config);
var client = factory.CreateChatClient("gpt-4o-mini");
```

**Output:**
```
🔍 Looking for provider to handle model: gpt-4o-mini
  ✓ Provider 'AzureOpenAI' handling model: gpt-4o-mini
```

**Result**: Azure OpenAI handles it (configured and available)

---

### Test 2: Open-Source Model → Falls Back to GitHub Models

```csharp
var factory = new ProviderFactory(config);
var client = factory.CreateChatClient("llama-3.2");
```

**Output:**
```
🔍 Looking for provider to handle model: llama-3.2
  ✓ Provider 'AzureOpenAI' handling model: llama-3.2
```

**Result**: Azure OpenAI tries first, but if it fails, OpenAI tries, then GitHub Models succeeds

---

### Test 3: Azure Down → Automatic Fallback

Simulate Azure being unavailable by removing credentials:

```json
{
  "providers": {
    "azureOpenAI": {
      "endpoint": "",  // Not configured
      "deploymentName": ""
    }
  }
}
```

```csharp
var client = factory.CreateChatClient("gpt-4o-mini");
```

**Output:**
```
🔍 Looking for provider to handle model: gpt-4o-mini
  ✗ Provider 'AzureOpenAI' failed to create client: Endpoint is not configured
  ✓ Provider 'OpenAI' handling model: gpt-4o-mini
```

**Result**: Automatically falls back to OpenAI

---

### Test 4: Multiple Agent Models

```csharp
// Agent 1: Uses gpt-4o-mini
var agent1 = new LoadedAgent { Model = "gpt-4o-mini" };
var client1 = factory.CreateChatClientForAgent(agent1);

// Agent 2: Uses llama-3.2
var agent2 = new LoadedAgent { Model = "llama-3.2" };
var client2 = factory.CreateChatClientForAgent(agent2);

// Agent 3: Uses o1-preview
var agent3 = new LoadedAgent { Model = "o1-preview" };
var client3 = factory.CreateChatClientForAgent(agent3);
```

**Output:**
```
🔍 Looking for provider to handle model: gpt-4o-mini
  ✓ Provider 'AzureOpenAI' handling model: gpt-4o-mini

🔍 Looking for provider to handle model: llama-3.2
  ✓ Provider 'AzureOpenAI' handling model: llama-3.2
  (or falls back to GitHubModels)

🔍 Looking for provider to handle model: o1-preview
  ✓ Provider 'AzureOpenAI' handling model: o1-preview
  (or falls back to OpenAI)
```

**Result**: Each agent gets routed to the appropriate provider

---

## Running the Example

1. **Update your appsettings.json** with the provider chain configuration

2. **Run the console application:**
   ```bash
   dotnet run -- interactive
   ```

3. **Select an agent** and observe the provider selection in the logs

4. **Try different models** by creating agents with different model names:
   ```markdown
   ---
   name: OpenAIAgent
   model: gpt-4o-mini
   ---
   ```

   ```markdown
   ---
   name: OpenSourceAgent
   model: llama-3.2
   ---
   ```

5. **Watch the chain in action!**

---

## Benefits Demonstrated

✅ **No code changes needed** - Just configure the chain order  
✅ **Automatic fallback** - Resilient to provider failures  
✅ **Cost optimization** - Use cheaper providers as fallbacks  
✅ **Model routing** - Different models automatically go to different providers  
✅ **Easy testing** - Disable providers by removing config, see chain handle it  

