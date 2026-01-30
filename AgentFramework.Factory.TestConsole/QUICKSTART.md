# Console Application - Quick Start Guide

## What's Been Created

The console application provides a complete template for loading and managing agents from markdown files.

### ✅ Features Implemented

1. **Configuration System**
   - JSON-based configuration (`appsettings.json`)
   - Environment variable support
   - Multi-provider configuration (Azure OpenAI, OpenAI, GitHub Models)
   - Per-agent overrides

2. **Markdown Parser**
   - Parses YAML frontmatter using Markdig + YamlDotNet
   - Extracts agent metadata (name, description, model, temperature, etc.)
   - Separates markdown body as agent instructions

3. **Agent Factory**
   - Loads agents from configuration
   - Discovers markdown files automatically
   - Applies configuration overrides
   - Validates and reports errors

4. **Interactive Console UI**
   - Displays loaded agents
   - Agent selection menu
   - Shows agent configuration and instructions
   - Professional formatting with box-drawing characters

## Project Structure

```
AgentFramework.Factory.TestConsole/
├── Program.cs                    # Main console application
├── Configuration.cs              # Configuration models
├── MarkdownAgentFactory.cs       # Agent factory implementation
├── appsettings.json             # Configuration file
├── sample-agent.md              # Example agent definition
└── CONFIG.md                    # Configuration documentation
```

## Running the Application

```bash
cd AgentFramework.Factory.TestConsole
dotnet run
```

### Expected Output

```
╔════════════════════════════════════════════════════════════════╗
║    Agent Framework Markdown Factory - Console Application     ║
╚════════════════════════════════════════════════════════════════╝

✓ Configuration loaded
  Agent Definitions Path: ./agents
  File Pattern: *.md
  Default Provider: azureOpenAI

✓ Agent Factory initialized

═══════════════════════════════════════════════════════════════
Loading Agents from Markdown Files
═══════════════════════════════════════════════════════════════

  ✓ Loaded: WeatherAssistant from ./sample-agent.md
✓ Successfully loaded 1 agent(s)

───────────────────────────────────────────────────────────────
Agent: WeatherAssistant
───────────────────────────────────────────────────────────────
  Description: A helpful weather assistant that provides weather information
  Model: gpt-4o-mini
  Temperature: 0,7
  Source: ./sample-agent.md
  Instructions Length: 1424 characters

═══════════════════════════════════════════════════════════════
Interactive Agent Demo
═══════════════════════════════════════════════════════════════

Select an agent to interact with:
  [1] WeatherAssistant - A helpful weather assistant that provides weather information
  [0] Exit
```

## Creating New Agents

### 1. Create a Markdown File

Create `my-agent.md` with YAML frontmatter:

```markdown
---
name: MyCustomAgent
description: Describe what your agent does
model: gpt-4o-mini
temperature: 0.7
---

# Persona

Define your agent's personality and role here.

## Responsibilities

- Task 1
- Task 2
- Task 3
```

### 2. Add to Configuration

Edit `appsettings.json`:

```json
{
  "agents": [
    {
      "name": "MyCustomAgent",
      "enabled": true,
      "markdownPath": "./my-agent.md",
      "overrides": {
        "temperature": 0.5
      }
    }
  ]
}
```

### 3. Run and Test

```bash
dotnet run
```

Your agent will appear in the selection menu!

## Configuration Options

See [CONFIG.md](CONFIG.md) for detailed configuration documentation.

### Quick Configuration

**Minimal Setup:**
```json
{
  "agentFactory": {
    "agentDefinitionsPath": "./agents"
  },
  "agents": [
    {
      "name": "MyAgent",
      "markdownPath": "./agents/my-agent.md"
    }
  ]
}
```

**With Provider:**
```json
{
  "agentFactory": {
    "defaultProvider": "azureOpenAI"
  },
  "providers": {
    "azureOpenAI": {
      "endpoint": "https://your-resource.openai.azure.com",
      "deploymentName": "gpt-4"
    }
  },
  "agents": [
    {
      "name": "MyAgent",
      "markdownPath": "./my-agent.md"
    }
  ]
}
```

## Next Steps to Implement

### 🚧 TODO: Actual Chat Integration

Currently, the application loads and displays agents but doesn't create actual Microsoft Agent Framework chat clients.

To complete the implementation:

1. **Add Provider Package**
   ```bash
   dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
   # or
   dotnet add package Microsoft.Agents.AI.AzureOpenAI --prerelease
   ```

2. **Create Chat Client Factory**
   ```csharp
   public class AgentChatClientFactory
   {
       public IChatClient CreateChatClient(LoadedAgent agent, AppConfiguration config)
       {
           // Create appropriate client based on provider
           // Apply agent configuration (model, temperature, instructions)
           // Return configured chat client
       }
   }
   ```

3. **Implement Chat Loop**
   ```csharp
   var chatClient = factory.CreateChatClient(selectedAgent, config);
   
   while (true)
   {
       Console.Write("You: ");
       var input = Console.ReadLine();
       
       if (string.IsNullOrEmpty(input) || input == "exit")
           break;
       
       var response = await chatClient.SendMessageAsync(input);
       Console.WriteLine($"Agent: {response}");
   }
   ```

## Key Classes

### `AppConfiguration`
Root configuration model that binds to `appsettings.json`

### `MarkdownAgentFactory`
Factory class that:
- Parses markdown files with YAML frontmatter
- Extracts agent metadata and instructions
- Applies configuration overrides
- Returns `LoadedAgent` objects

### `LoadedAgent`
Model representing a fully configured agent with:
- Name, description, model settings
- Instructions extracted from markdown
- Provider information
- Source file reference

## Example Agent Metadata

```yaml
---
name: CodeReviewAgent
description: Reviews code and provides improvement suggestions
model: gpt-4o-mini
temperature: 0.2
max_tokens: 4000
top_p: 1.0
frequency_penalty: 0.0
presence_penalty: 0.0
---
```

All YAML properties are optional except `name` and `description`. Defaults from configuration or code will be used.

## Error Handling

The application includes comprehensive error handling:

- **File not found**: Reports missing markdown files
- **Invalid YAML**: Shows YAML parsing errors
- **Configuration errors**: Displays configuration validation issues
- **Runtime errors**: Catches and displays exceptions with stack traces

## Development Tips

1. **Use `appsettings.Development.json`** for local development settings
2. **Store secrets in environment variables**, not in JSON files
3. **Enable logging** during development: `"logLevel": "Debug"`
4. **Use auto-reload** for rapid iteration: `"autoReload": true`

## Security Best Practices

✅ **DO:**
- Use environment variables for API keys
- Add `appsettings.*.Local.json` to `.gitignore`
- Use Azure Managed Identity or similar for production

❌ **DON'T:**
- Commit API keys to source control
- Store secrets in `appsettings.json`
- Share configuration files with credentials

## Testing

Test the parser independently:

```csharp
var factory = new MarkdownAgentFactory(config);
var agent = factory.LoadAgentFromFile("test-agent.md");

Console.WriteLine($"Name: {agent.Name}");
Console.WriteLine($"Model: {agent.Model}");
Console.WriteLine($"Instructions: {agent.Instructions}");
```

---

**Status**: ✅ Configuration and parsing complete | 🚧 Chat client integration pending

**Last Updated**: 2026-01-30
