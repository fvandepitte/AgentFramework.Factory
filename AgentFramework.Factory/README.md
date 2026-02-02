# AgentFramework.Factory

Core library for creating AI agents from markdown definitions, extending the Microsoft Agent Framework with GitHub Copilot-style markdown agent support.

## Overview

This library provides a reusable framework for defining AI agents using markdown files with YAML frontmatter. It bridges the gap between .NET's code-first approach and Python's declarative agent definitions.

## Features

- 📝 **Markdown-based agent definitions** - Define agents using markdown files with YAML frontmatter
- 🔌 **Provider abstraction** - Support for multiple LLM providers (Azure OpenAI, OpenAI, GitHub Models)
- 🛠️ **Tool integration** - Extensible tool provider system
- ⚙️ **Flexible configuration** - Layered configuration with overrides
- 🔗 **Chain of Responsibility** - Automatic provider fallback and routing
- 🏗️ **Fluent API** - Builder pattern for programmatic agent creation

## Installation

```bash
dotnet add package AgentFramework.Factory
```

## Quick Start

### 1. Define an agent in markdown

```markdown
---
name: WeatherAssistant
description: Helps users get weather information
model: gpt-4o-mini
temperature: 0.7
max_tokens: 2000
tools: ["get_weather"]
---

# Weather Assistant

You are a helpful weather assistant. Use the get_weather tool to provide
accurate weather information to users.
```

### 2. Configure and register services

```csharp
using AgentFramework.Factory.Extensions;

var services = new ServiceCollection();

services.AddAgentFramework(configuration)
    .AddMarkdownAgents(options => 
    {
        options.AgentDefinitionsPath = "./agents";
    })
    .AddProvider<AzureOpenAIProviderHandler>()
    .AddToolProvider<LocalToolProvider>();
```

### 3. Load and use agents

```csharp
var factory = serviceProvider.GetRequiredService<IMarkdownAgentFactory>();
var agent = factory.LoadAgentFromFile("./agents/weather-assistant.md");

Console.WriteLine($"Loaded agent: {agent.Name}");
Console.WriteLine($"Model: {agent.Model}");
```

## Architecture

### Core Abstractions

- **IMarkdownAgentFactory** - Factory for loading agents from markdown
- **ILoadedAgent** - Represents a loaded agent with configuration
- **IProviderHandler** - Handler for creating LLM chat clients
- **IToolProvider** - Provider for agent tools
- **IAgentBuilder** - Fluent API for programmatic agent creation
- **IAgentRepository** - Repository for loading/saving agents
- **IAgentRunner** - Execute conversations with agents

### Core Models

- **AgentMetadata** - YAML frontmatter metadata
- **LoadedAgent** - Runtime agent representation
- **AgentValidationResult** - Validation results
- **AgentFactoryConfiguration** - Factory configuration

### Exceptions

- **AgentLoadException** - Agent loading failures
- **ProviderNotFoundException** - Provider not found
- **ToolResolutionException** - Tool resolution failures

## Configuration

```json
{
  "agentFactory": {
    "agentDefinitionsPath": "./agents",
    "agentFilePattern": "*.md",
    "defaultProvider": "azureOpenAI",
    "providerChain": ["azureOpenAI", "openAI", "githubModels"],
    "enableLogging": true,
    "enableToolDiscovery": true
  }
}
```

## Provider Packages (Coming Soon)

- `AgentFramework.Factory.Provider.AzureOpenAI`
- `AgentFramework.Factory.Provider.OpenAI`
- `AgentFramework.Factory.Provider.GitHubModels`

## License

MIT License - see LICENSE file for details

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
