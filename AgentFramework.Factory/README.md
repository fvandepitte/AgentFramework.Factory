# AgentFramework.Factory

Core abstractions and interfaces for the Agent Framework Factory library system.

## Overview

This library provides the foundational interfaces and abstractions for building extensible AI agent systems using markdown-based agent definitions. It enables provider-agnostic agent creation with automatic provider selection using the Chain of Responsibility pattern.

## Key Interfaces

### IProviderHandler

Defines handlers for creating chat clients based on model names.

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

### IToolProvider

Defines providers for supplying AI tools to agents.

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

### IMarkdownAgentFactory

Defines factory for loading agents from markdown files.

```csharp
public interface IMarkdownAgentFactory
{
    ILoadedAgent LoadAgentFromFile(string markdownPath, string? provider = null);
    ILoadedAgent ParseMarkdown(string markdownContent, string? provider = null);
}
```

### ILoadedAgent

Represents an agent loaded from markdown with all configuration.

```csharp
public interface ILoadedAgent
{
    string Name { get; }
    string Description { get; }
    string Model { get; }
    string Instructions { get; }
    IReadOnlyList<string> Tools { get; }
    // ... additional properties
}
```

## Installation

```bash
dotnet add package AgentFramework.Factory
```

## Usage

Implement the interfaces in your custom providers:

```csharp
public class CustomProviderHandler : IProviderHandler
{
    public string ProviderName => "custom";
    
    public bool CanHandle(string modelName)
    {
        return modelName.StartsWith("custom-");
    }
    
    public IChatClient? CreateChatClient(string modelName)
    {
        // Your implementation
    }
    
    // Implement other interface members
}
```

## Dependencies

- Microsoft.Extensions.AI.Abstractions (10.0.0)

## Documentation

See [EXTENSIBILITY.md](EXTENSIBILITY.md) for detailed extensibility guide.

## License

See LICENSE file in repository root.
