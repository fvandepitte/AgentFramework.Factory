# AgentFramework.Factory - Extensibility Guide

## Overview

The AgentFramework.Factory library provides a modular, extensible architecture for creating and managing AI agents using markdown definitions. This guide explains how to extend the framework with custom providers, tools, and configurations.

## Architecture

The framework is organized into two main components:

1. **AgentFramework.Factory** (Core Library) - Contains abstractions and interfaces
2. **AgentFramework.Factory.TestConsole** - Reference implementation and CLI tools

## Core Abstractions

### IProviderHandler

Defines a handler for creating chat clients based on model names. Implements the Chain of Responsibility pattern for automatic provider fallback.

```csharp
public interface IProviderHandler
{
    string ProviderName { get; }
    bool CanHandle(string modelName);
    IChatClient? CreateChatClient(string modelName);
    IProviderHandler SetNext(IProviderHandler nextHandler);
    IChatClient? Handle(string modelName, ...);
}
```

**Implementation Example:**

```csharp
public class CustomProviderHandler : BaseProviderHandler
{
    public override string ProviderName => "customProvider";

    public override bool CanHandleModel(string modelName)
    {
        // Return true if this provider supports the model
        return modelName.StartsWith("custom-");
    }

    public override IChatClient CreateChatClient(string modelName)
    {
        // Create and return your IChatClient implementation
        return new CustomChatClient(modelName);
    }
}
```

**Registration:**

```csharp
services.AddSingleton<IProviderHandler, CustomProviderHandler>();
```

### IToolProvider

Defines a provider that supplies AI tools to agents.

```csharp
public interface IToolProvider
{
    string Name { get; }
    string Type { get; }
    bool CanProvide(string toolName);
    IEnumerable<AITool> GetTools(IEnumerable<string> toolNames);
    IEnumerable<AITool> GetAllTools();
}
```

**Implementation Example:**

```csharp
public class CustomToolProvider : IToolProvider
{
    public string Name => "CustomTools";
    public string Type => "custom";

    public bool CanProvide(string toolName)
    {
        return toolName.StartsWith("custom_");
    }

    public IEnumerable<AITool> GetTools(IEnumerable<string> toolNames)
    {
        foreach (var toolName in toolNames.Where(CanProvide))
        {
            yield return CreateTool(toolName);
        }
    }

    public IEnumerable<AITool> GetAllTools()
    {
        yield return CreateTool("custom_tool1");
        yield return CreateTool("custom_tool2");
    }

    private AITool CreateTool(string name)
    {
        // Create and return your AITool implementation
    }
}
```

**Registration:**

```csharp
services.AddSingleton<IToolProvider, CustomToolProvider>();
```

### IMarkdownAgentFactory

Defines a factory for loading agents from markdown files.

```csharp
public interface IMarkdownAgentFactory
{
    ILoadedAgent LoadAgentFromFile(string markdownPath, string? provider = null);
    ILoadedAgent ParseMarkdown(string markdownContent, string? provider = null);
}
```

### ILoadedAgent

Represents an agent loaded from markdown with all its configuration.

```csharp
public interface ILoadedAgent
{
    string Name { get; }
    string Description { get; }
    string Model { get; }
    string Instructions { get; }
    IReadOnlyList<string> Tools { get; }
    double Temperature { get; }
    int? MaxTokens { get; }
    // ... other properties
}
```

## Configuration Extensibility

### Using Options Callbacks

The framework supports configuration customization through callbacks:

```csharp
services.AddAgentFactoryServices(options =>
{
    options.ConfigFilePath = "custom-config.json";
    
    options.ConfigureConfiguration = builder =>
    {
        // Add custom configuration sources
        builder.AddJsonFile("extra-config.json", optional: true);
        builder.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["agentFactory:defaultProvider"] = "customProvider"
        });
    };
    
    options.ConfigureLogging = logConfig =>
    {
        // Customize logging
        logConfig.MinimumLevel.Debug()
                .WriteTo.Console();
    };
});
```

### Direct Method Overloads

```csharp
services.AddAgentFactoryServices(
    configFilePath: "custom-config.json",
    configureConfiguration: builder => { /* customize */ },
    configureLogging: logConfig => { /* customize */ }
);
```

## Provider Chain Configuration

The framework uses a Chain of Responsibility pattern for provider selection:

```json
{
  "agentFactory": {
    "defaultProvider": "azureOpenAI",
    "providerChain": ["azureOpenAI", "openAI", "customProvider"]
  }
}
```

When creating a chat client:
1. The first provider in the chain tries to handle the model
2. If it can't, the next provider is tried
3. This continues until a provider succeeds or the chain ends

## Creating Custom Provider Packs

### Step 1: Create a New Library

```bash
dotnet new classlib -n AgentFramework.Factory.Providers.Custom
```

### Step 2: Add Reference to Core Library

```xml
<ItemGroup>
  <ProjectReference Include="..\AgentFramework.Factory\AgentFramework.Factory.csproj" />
</ItemGroup>
```

### Step 3: Implement IProviderHandler

```csharp
using AgentFramework.Factory.Abstractions;

public class CustomProviderHandler : IProviderHandler
{
    public string ProviderName => "custom";
    
    // Implement all interface members
}
```

### Step 4: Create Extension Method for Registration

```csharp
public static class CustomProviderExtensions
{
    public static IServiceCollection AddCustomProvider(
        this IServiceCollection services,
        Action<CustomProviderOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }
        
        services.AddSingleton<IProviderHandler, CustomProviderHandler>();
        return services;
    }
}
```

### Step 5: Use in Applications

```csharp
services.AddAgentFactoryServices()
        .AddCustomProvider(options =>
        {
            options.ApiKey = "...";
            options.Endpoint = "...";
        });
```

## Testing Your Extensions

### Unit Testing Provider Handlers

```csharp
[Fact]
public void ProviderHandler_CanHandle_ReturnsTrue_ForSupportedModels()
{
    var provider = new CustomProviderHandler(options);
    
    var canHandle = provider.CanHandle("custom-model");
    
    Assert.True(canHandle);
}

[Fact]
public void ProviderHandler_CreateChatClient_ReturnsClient()
{
    var provider = new CustomProviderHandler(options);
    
    var client = provider.CreateChatClient("custom-model");
    
    Assert.NotNull(client);
    Assert.IsAssignableFrom<IChatClient>(client);
}
```

### Integration Testing

```csharp
[Fact]
public async Task AgentFactory_CreatesAgent_WithCustomProvider()
{
    var services = new ServiceCollection();
    services.AddAgentFactoryServices()
            .AddCustomProvider();
    
    var sp = services.BuildServiceProvider();
    var factory = sp.GetRequiredService<AgentFactory>();
    
    var agent = factory.CreateAgentFromMarkdown("agent.md", "custom");
    
    Assert.NotNull(agent);
}
```

## Best Practices

1. **Follow Naming Conventions**: Use camelCase for private fields without underscore prefix
2. **Implement Proper Logging**: Use ILogger<T> for consistent logging
3. **Handle Errors Gracefully**: Throw meaningful exceptions with context
4. **Support Configuration**: Accept options via IOptions<T> pattern
5. **Document Your Extensions**: Provide XML comments for public APIs
6. **Test Thoroughly**: Write unit and integration tests

## Examples

See the following implementations in the TestConsole project:

- `AzureOpenAIProviderHandler` - Azure OpenAI provider implementation
- `OpenAIProviderHandler` - OpenAI provider implementation
- `GitHubModelsProviderHandler` - GitHub Models provider implementation
- `LocalToolProvider` - Local tool provider implementation
- `McpToolProvider` - MCP (Model Context Protocol) tool provider implementation

## Support

For questions or issues, please refer to the main README.md or create an issue in the repository.
