# Agent Framework Markdown Factory - AI Coding Assistant Guide

## Project Overview

This is an **innovation/research project** that extends the Microsoft Agent Framework to support **GitHub Copilot-style markdown agent definitions** for .NET. The project enables defining AI agents using markdown files with YAML frontmatter, bridging the gap between .NET's limited declarative support and Python's full YAML capabilities.

### Core Architecture

**Two main components:**
1. **AgentFramework.Factory** - Core library (currently minimal, future extensibility)
2. **AgentFramework.Factory.TestConsole** - CLI application for loading and testing markdown-defined agents

**Key design pattern:** Markdown files (`*.md`) with YAML frontmatter define agent metadata, while the markdown body contains agent instructions/persona. Configuration overrides are applied through `appsettings.json`.

## Critical Developer Knowledge

### Markdown Agent Definition Format

Agents are defined using this specific structure (see [sample-agent.md](../AgentFramework.Factory.TestConsole/sample-agent.md)):

```markdown
---
name: AgentName
description: Brief description
model: gpt-4o-mini
temperature: 0.7
max_tokens: 2000
top_p: 1.0
frequency_penalty: 0.0
presence_penalty: 0.0
---

# Persona
Agent instructions and persona definition in markdown...
```

**Parsing stack:** Markdig + YAML frontmatter extension → YamlDotNet deserializer → `AgentMetadata` POCO

### Configuration System

Configuration follows a **layered override pattern** (see [CONFIG.md](../AgentFramework.Factory.TestConsole/CONFIG.md)):

1. Base configuration in `appsettings.json`
2. Environment-specific overrides (`appsettings.{Environment}.json`)
3. User secrets (via `dotnet user-secrets`, especially for API keys)
4. Environment variables
5. Per-agent configuration overrides in `agents` section

**Critical:** API keys should NEVER be committed. Use user secrets or environment variables:
```bash
dotnet user-secrets set "providers:azureOpenAI:apiKey" "your-key"
```

### Build & Run Commands

```bash
# Run with default configuration
dotnet run

# Run specific commands
dotnet run -- list --verbose
dotnet run -- show AgentName
dotnet run -- read-test path/to/agent.md
dotnet run -- interactive

# Initialize user secrets (one-time)
dotnet user-secrets init
```

**No build configuration required** - standard .NET 10 console application.

## Project-Specific Patterns

### Service Classes Organization

All core services live in [Services/](../AgentFramework.Factory.TestConsole/Services/):
- **ConfigurationLoader** - Loads layered configuration using `Microsoft.Extensions.Configuration`
- **MarkdownAgentFactory** - Orchestrates markdown parsing and agent loading
- **Configuration.cs** - Complete configuration model hierarchy (all POCOs)
- **LoadedAgent.cs** - Runtime agent representation after merging metadata + config overrides

### Command Pattern (Spectre.Console.Cli)

Commands are in [Commands/](../AgentFramework.Factory.TestConsole/Commands/) and follow this pattern:
```csharp
public class MyCommand : Command<MyCommand.Settings>
{
    public class Settings : CommandSettings 
    {
        [CommandOption("--option")]
        public string Option { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        var config = ConfigurationLoader.LoadConfiguration(settings.ConfigFile);
        // Command logic...
        return 0;
    }
}
```

All commands support `--config` option for custom configuration files.

### YAML Parsing Convention

YamlDotNet attributes use snake_case aliases to match common agent definition formats:
```csharp
[YamlMember(Alias = "max_tokens")]
public int? MaxTokens { get; set; }
```

This allows markdown frontmatter to use `max_tokens` while C# uses `MaxTokens`.

## Dependencies & Why They're Used

- **Markdig** - Fast, extensible markdown parser with YAML frontmatter support
- **YamlDotNet** - Robust YAML deserializer for frontmatter extraction
- **Spectre.Console** - Beautiful CLI framework with tables, panels, and styling
- **Spectre.Console.Cli** - Command routing and argument parsing
- **Microsoft.Extensions.Configuration** - Layered configuration with JSON, env vars, user secrets

## Integration Points

**Currently isolated** - This is a proof-of-concept CLI application. Future integration points:
- Agent definitions → Microsoft Agent Framework SDK instantiation (not yet implemented)
- LLM provider connections (Azure OpenAI, OpenAI, GitHub Models) - configured but not connected
- File watching (`autoReload` config) - planned, not implemented

## Common Pitfalls

1. **Forgetting to set user secrets** - If API keys are missing, configure them:
   ```bash
   dotnet user-secrets set "providers:azureOpenAI:apiKey" "key"
   dotnet user-secrets set "providers:azureOpenAI:endpoint" "https://..."
   ```

2. **YAML frontmatter must use `---` delimiters** - Both opening and closing delimiters required

3. **Agent names in configuration must match frontmatter** - The `name` field in `agents[]` config should match the YAML `name:` field or use overrides

4. **File paths are relative to working directory** - `agentDefinitionsPath` is relative to where you run `dotnet run`, not the project directory

## Documentation Structure

- [README.md](../README.md) - Project goals, inspiration (GitHub Copilot + MS Agent Framework)
- [DECLARATIVE_SUPPORT.md](../DECLARATIVE_SUPPORT.md) - Analysis of .NET vs Python declarative capabilities
- [QUICKSTART.md](../AgentFramework.Factory.TestConsole/QUICKSTART.md) - Getting started guide
- [CLI.md](../AgentFramework.Factory.TestConsole/CLI.md) - Complete CLI command reference
- [CONFIG.md](../AgentFramework.Factory.TestConsole/CONFIG.md) - Configuration documentation

## Key Files to Understand

Start here to understand the codebase:
1. [MarkdownAgentFactory.cs](../AgentFramework.Factory.TestConsole/Services/MarkdownAgentFactory.cs) - Core parsing logic
2. [Configuration.cs](../AgentFramework.Factory.TestConsole/Services/Configuration.cs) - Complete data model
3. [sample-agent.md](../AgentFramework.Factory.TestConsole/sample-agent.md) - Example agent definition
4. [appsettings.json](../AgentFramework.Factory.TestConsole/appsettings.json) - Configuration schema example
