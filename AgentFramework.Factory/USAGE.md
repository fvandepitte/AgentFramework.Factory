# Example: Using AgentFramework.Factory Core Library

This example demonstrates how to use the `AgentFramework.Factory` core library in your own application.

## Installation

```bash
dotnet add package AgentFramework.Factory
```

## Basic Usage

### 1. Register Services

```csharp
using AgentFramework.Factory.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Create service collection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

// Register Agent Framework Factory
services.AddAgentFramework(configuration)
    .AddMarkdownAgents(options =>
    {
        options.AgentDefinitionsPath = "./agents";
        options.AgentFilePattern = "*.md";
    });

// Build service provider
var serviceProvider = services.BuildServiceProvider();
```

### 2. Load an Agent

```csharp
using AgentFramework.Factory.Abstractions;

var factory = serviceProvider.GetRequiredService<IMarkdownAgentFactory>();

// Load from file
var agent = factory.LoadAgentFromFile("./agents/my-agent.md");

Console.WriteLine($"Loaded: {agent.Name}");
Console.WriteLine($"Description: {agent.Description}");
Console.WriteLine($"Model: {agent.Model}");
Console.WriteLine($"Temperature: {agent.Temperature}");
Console.WriteLine($"Tools: {string.Join(", ", agent.Tools)}");
```

### 3. Parse Markdown Content

```csharp
string markdownContent = @"---
name: MyAgent
description: A helpful assistant
model: gpt-4o-mini
temperature: 0.7
tools: [""search"", ""calculator""]
---

# My Agent

You are a helpful assistant.
";

var agent = factory.ParseMarkdown(markdownContent);
```

### 4. Use with Custom Provider

To use the agent with a custom provider, you'll need to implement `IProviderHandler`:

```csharp
using AgentFramework.Factory.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

public class MyCustomProvider : IProviderHandler
{
    private readonly ILogger<MyCustomProvider> logger;

    public MyCustomProvider(ILogger<MyCustomProvider> logger)
    {
        this.logger = logger;
    }

    public string ProviderName => "myCustomProvider";

    public bool CanHandle(string modelName)
    {
        // Return true if this provider can handle the model
        return modelName.StartsWith("my-model-");
    }

    public IChatClient? CreateChatClient(string modelName)
    {
        // Create and return your chat client
        // Return null if unable to create
        return null;
    }

    public IProviderHandler SetNext(IProviderHandler nextHandler)
    {
        // Chain of Responsibility pattern implementation
        return nextHandler;
    }

    public IChatClient? Handle(
        string modelName,
        Action<IProviderHandler, string>? onSuccess = null,
        Action<IProviderHandler, string, Exception>? onFailure = null)
    {
        // Handle the request or pass to next handler
        return null;
    }
}
```

Then register it:

```csharp
services.AddAgentFramework(configuration)
    .AddProvider<MyCustomProvider>();
```

## Configuration

Create an `appsettings.json` file:

```json
{
  "agentFactory": {
    "agentDefinitionsPath": "./agents",
    "agentFilePattern": "*.md",
    "defaultProvider": "azureOpenAI",
    "enableLogging": true,
    "enableToolDiscovery": true
  },
  "tools": {
    "enableMcp": false,
    "registeredTools": []
  }
}
```

## Agent Markdown Format

Create agent definitions in markdown files with YAML frontmatter:

```markdown
---
name: MyAgent
description: A helpful assistant
model: gpt-4o-mini
temperature: 0.7
max_tokens: 2000
top_p: 1.0
tools: ["search", "calculator"]
---

# Agent Instructions

You are a helpful assistant that can search the web and perform calculations.

When the user asks a question:
1. Determine if you need to search for information
2. Use the calculator for mathematical questions
3. Provide clear, concise answers
```

## Advanced Usage

### Implementing IAgentBuilder

The core library includes `IAgentBuilder` for programmatic agent creation:

```csharp
// This interface is available for custom implementations
// You can create your own builder that implements IAgentBuilder
```

### Implementing IAgentRepository

For loading agents from databases or other sources:

```csharp
using AgentFramework.Factory.Abstractions;

public class DatabaseAgentRepository : IAgentRepository
{
    public Task<ILoadedAgent?> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        // Load from database
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ILoadedAgent>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        // Load all from database
        throw new NotImplementedException();
    }

    public Task SaveAsync(ILoadedAgent agent, CancellationToken cancellationToken = default)
    {
        // Save to database
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        // Delete from database
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        // Check existence in database
        throw new NotImplementedException();
    }
}
```

Then register it:

```csharp
services.AddSingleton<IAgentRepository, DatabaseAgentRepository>();
```

## Error Handling

The library provides custom exceptions:

```csharp
using AgentFramework.Factory.Exceptions;

try
{
    var agent = factory.LoadAgentFromFile("./agents/missing.md");
}
catch (AgentLoadException ex)
{
    Console.WriteLine($"Failed to load agent: {ex.Message}");
    Console.WriteLine($"File: {ex.FilePath}");
}
catch (ProviderNotFoundException ex)
{
    Console.WriteLine($"Provider not found: {ex.ProviderName}");
}
```

## See Also

- [AgentFramework.Factory.TestConsole](../AgentFramework.Factory.TestConsole/) - Reference implementation
- [librarystructure.md](../librarystructure.md) - Library architecture and design
