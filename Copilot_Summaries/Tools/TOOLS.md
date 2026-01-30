# Tool Support Documentation

## Overview

The Agent Framework Factory now supports **tool integration** through dependency injection, enabling agents to access external capabilities via local C# methods or Model Context Protocol (MCP) servers.

## Features

- **Local Tool Discovery**: Automatically discovers tools from C# methods decorated with `[Description]` attributes
- **Dependency Injection**: Fully integrated with .NET DI container for modular, testable tool implementations
- **Multiple Tool Providers**: Supports local tools and MCP tools through a provider pattern
- **Markdown Configuration**: Define agent tools directly in markdown frontmatter
- **Chain of Responsibility**: Tools are resolved through multiple providers automatically

## Architecture

### Components

1. **IToolProvider**: Interface for tool providers (local, MCP, etc.)
2. **LocalToolProvider**: Discovers and provides tools from C# methods
3. **McpToolProvider**: Provides tools from MCP servers (placeholder for future implementation)
4. **ToolFactory**: Manages tool providers and resolves tools for agents
5. **AgentFactory**: Integrates tools into agent creation process

### Tool Discovery Flow

```
Markdown Agent Definition
    ↓
MarkdownAgentFactory parses tool names
    ↓
AgentFactory.CreateAgent() requests tools
    ↓
ToolFactory queries registered IToolProviders
    ↓
LocalToolProvider discovers C# methods with [Description]
    ↓
Tools are attached to ChatOptions
    ↓
AIAgent created with tools
```

## Defining Tools in Markdown

Add a `tools` array to your agent's YAML frontmatter:

```yaml
---
name: WeatherAssistant
description: A helpful weather assistant
model: gpt-4o-mini
temperature: 0.7
tools:
  - GetCurrentWeather
  - GetWeatherForecast
  - GetClothingRecommendation
---
```

## Creating Local Tools

### Basic Tool Definition

Create a static or instance method decorated with `[Description]`:

```csharp
using System.ComponentModel;

public class WeatherTools
{
    [Description("Gets the current weather for a specified location.")]
    public static string GetCurrentWeather(
        [Description("The city name or location")] string location)
    {
        // Implementation
        return $"Weather in {location}: 22°C, Sunny";
    }
}
```

### Key Requirements

1. **Description Attribute**: Required on the method (explains what the tool does)
2. **Parameter Descriptions**: Optional but recommended for better LLM understanding
3. **Static or Instance**: 
   - Static methods work out of the box
   - Instance methods require the class to be registered in DI

### Tool Naming

- Tool names are derived from method names (e.g., `GetCurrentWeather`)
- Case-sensitive matching between markdown and method names
- Keep names descriptive and action-oriented (GetX, CalculateY, FetchZ)

## Configuration

### appsettings.json

```json
{
  "agentFactory": {
    "enableToolDiscovery": true,
    "toolAssemblies": [],
    "enableLogging": true
  },
  "tools": {
    "enableMcp": false,
    "registeredTools": []
  },
  "mcpServers": []
}
```

### Configuration Options

- **enableToolDiscovery**: When `true`, automatically scans assemblies for tools
- **toolAssemblies**: Additional assemblies to scan for tools (beyond the executing assembly)
- **enableMcp**: Enable/disable MCP tool provider
- **mcpServers**: Configuration for MCP servers (future implementation)

## Dependency Injection Setup

Tools are automatically registered during service configuration:

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<IToolProvider, LocalToolProvider>();
services.AddSingleton<IToolProvider, McpToolProvider>();
services.AddSingleton<ToolFactory>();
```

### Registering Custom Tool Classes

For instance methods, register your tool class in DI:

```csharp
services.AddSingleton<MyCustomTools>();
```

Then the `LocalToolProvider` will automatically discover instance methods from registered services.

## Using Tools in Agents

### Tool Resolution

When an agent is created:

1. `MarkdownAgentFactory` loads tool names from frontmatter
2. `AgentFactory` calls `ToolFactory.GetToolsForAgent(toolNames)`
3. `ToolFactory` queries each `IToolProvider` via `CanProvide(toolName)`
4. First matching provider returns the tool via `GetTools()`
5. Tools are added to `ChatOptions.Tools`
6. Agent is created with tools available for LLM function calling

### Tool Invocation

Tools are invoked automatically by the LLM during conversation:

1. User asks a question requiring external data
2. LLM decides to call a tool
3. Microsoft.Extensions.AI handles parameter binding and execution
4. Tool result is provided back to the LLM
5. LLM generates final response using tool data

## Example: Complete Weather Agent

### 1. Define Tools (WeatherTools.cs)

```csharp
using System.ComponentModel;

namespace AgentFramework.Factory.TestConsole.Tools.Samples;

public class WeatherTools
{
    [Description("Gets the current weather for a specified location.")]
    public static string GetCurrentWeather(
        [Description("The city name or location")] string location)
    {
        return $"Weather in {location}: 22°C, Sunny, Humidity: 60%";
    }

    [Description("Gets a weather forecast for the next days.")]
    public static string GetWeatherForecast(
        [Description("The city name")] string location,
        [Description("Number of days (1-10)")] int days = 5)
    {
        var forecast = $"Weather forecast for {location} ({days} days):\n";
        for (int i = 1; i <= days; i++)
        {
            forecast += $"  Day {i}: 20°C, Partly Cloudy\n";
        }
        return forecast;
    }
}
```

### 2. Define Agent (weather-agent.md)

```markdown
---
name: WeatherAssistant
description: A helpful weather assistant
model: gpt-4o-mini
temperature: 0.7
tools:
  - GetCurrentWeather
  - GetWeatherForecast
---

# Persona

You are a friendly weather assistant with access to real-time weather data.

Use the GetCurrentWeather tool to provide current conditions.
Use the GetWeatherForecast tool to provide multi-day forecasts.

Always be helpful and provide actionable advice.
```

### 3. Configure Agent (appsettings.json)

```json
{
  "agents": [
    {
      "name": "WeatherAssistant",
      "enabled": true,
      "markdownPath": "./agents/weather-agent.md"
    }
  ]
}
```

### 4. Test the Agent

```bash
dotnet run -- test-agent WeatherAssistant --message "What's the weather in Seattle?"
```

## Tool Provider Pattern

### Creating Custom Tool Providers

Implement `IToolProvider` to create custom tool sources:

```csharp
public class MyCustomToolProvider : IToolProvider
{
    public string Name => "MyProvider";
    public string Type => "custom";

    public bool CanProvide(string toolName)
    {
        // Check if this provider handles the tool
        return _myTools.ContainsKey(toolName);
    }

    public IEnumerable<AITool> GetTools(IEnumerable<string> toolNames)
    {
        // Return requested tools
        foreach (var name in toolNames)
        {
            if (_myTools.TryGetValue(name, out var tool))
                yield return tool;
        }
    }

    public IEnumerable<AITool> GetAllTools()
    {
        return _myTools.Values;
    }
}
```

Register in DI:

```csharp
services.AddSingleton<IToolProvider, MyCustomToolProvider>();
```

## Advanced Scenarios

### Tool Discovery from Multiple Assemblies

```json
{
  "agentFactory": {
    "toolAssemblies": [
      "MyCompany.WeatherTools",
      "MyCompany.DatabaseTools",
      "ThirdParty.CustomTools"
    ]
  }
}
```

### Conditional Tool Loading

Use agent overrides to enable different tools per environment:

```json
{
  "agents": [
    {
      "name": "WeatherAssistant",
      "enabled": true,
      "markdownPath": "./agents/weather-agent.md",
      "overrides": {
        "tools": ["GetCurrentWeather"] // Override tools from markdown
      }
    }
  ]
}
```

### Tool Validation

The `ToolFactory` logs warnings for tools that cannot be resolved:

```
✓ Resolved tool 'GetCurrentWeather' from Local provider
⚠ Could not find tools: NonExistentTool
```

## Best Practices

### Tool Design

1. **Single Responsibility**: Each tool should do one thing well
2. **Descriptive Names**: Use clear, action-oriented method names
3. **Rich Descriptions**: Provide detailed descriptions for LLM understanding
4. **Type Safety**: Use strong types for parameters (LLM will handle conversion)
5. **Error Handling**: Return error messages as strings rather than throwing exceptions

### Performance

1. **Static Methods**: Prefer static methods for stateless tools (no DI overhead)
2. **Caching**: Cache expensive operations within tools
3. **Async Operations**: Use async methods for I/O-bound operations

### Security

1. **Input Validation**: Always validate tool parameters
2. **Sandboxing**: Limit what tools can access (files, network, databases)
3. **Audit Logging**: Log tool invocations for security auditing
4. **Rate Limiting**: Implement rate limits for expensive operations

## Troubleshooting

### Tool Not Found

**Problem**: `⚠ Could not find tools: MyTool`

**Solutions**:
- Verify method name matches exactly (case-sensitive)
- Ensure `[Description]` attribute is present
- Check that the class is in a scanned assembly
- For instance methods, verify the class is registered in DI

### Tool Not Being Called

**Problem**: LLM doesn't use available tools

**Solutions**:
- Improve tool descriptions to clarify when to use them
- Update agent instructions to mention available tools
- Ensure tool parameters are clearly described
- Check that the LLM model supports function calling

### Build Errors

**Problem**: `Cannot convert from 'AIFunction' to 'Delegate'`

**Solution**: Ensure you're using the latest version of Microsoft.Extensions.AI

## Future Enhancements

### MCP Integration (Planned)

```json
{
  "tools": {
    "enableMcp": true
  },
  "mcpServers": [
    {
      "name": "filesystem",
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "/workspace"]
    }
  ]
}
```

### Dynamic Tool Registration

```csharp
// Future API
toolFactory.RegisterTool("CustomTool", customFunction);
```

### Tool Composition

```yaml
tools:
  - name: ComplexWorkflow
    composition:
      - GetCurrentWeather
      - AnalyzeData
      - GenerateReport
```

## Resources

- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [Sample Tools](./Tools/Samples/)

## Support

For issues or questions:
1. Check troubleshooting section above
2. Review example tools in `Tools/Samples/`
3. Enable verbose logging in `appsettings.json`
4. Create an issue on GitHub

---

**Last Updated**: 2026-01-30  
**Version**: 1.0.0
