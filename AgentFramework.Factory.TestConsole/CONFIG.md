# Agent Factory Configuration Guide

This document explains the `appsettings.json` configuration options for the Agent Framework Markdown Factory.

## Configuration Structure

### `agentFactory` Section

Main configuration section for the agent factory behavior.

```json
{
  "agentFactory": {
    "agentDefinitionsPath": "./agents",
    "agentFilePattern": "*.md",
    "outputPath": "./generated",
    "autoReload": false,
    "defaultProvider": "azureOpenAI",
    "enableLogging": true,
    "logLevel": "Information"
  }
}
```

#### Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `agentDefinitionsPath` | string | Directory path where agent markdown files are located | `"./agents"` |
| `agentFilePattern` | string | Glob pattern to match agent files | `"*.md"` |
| `outputPath` | string | Directory for generated agent artifacts (optional) | `"./generated"` |
| `autoReload` | boolean | Watch for file changes and reload agents automatically | `false` |
| `defaultProvider` | string | Default LLM provider to use (`azureOpenAI`, `openAI`, `githubModels`) | `"azureOpenAI"` |
| `enableLogging` | boolean | Enable detailed logging | `true` |
| `logLevel` | string | Logging level (`Trace`, `Debug`, `Information`, `Warning`, `Error`) | `"Information"` |

---

### `providers` Section

Configuration for different LLM providers.

#### Azure OpenAI

```json
{
  "providers": {
    "azureOpenAI": {
      "endpoint": "https://your-resource.openai.azure.com",
      "apiKey": "<your-api-key-or-use-env-var>",
      "deploymentName": "gpt-4o-mini",
      "apiVersion": "2024-08-01-preview"
    }
  }
}
```

**Environment Variables Alternative:**
- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_DEPLOYMENT_NAME`
- `AZURE_OPENAI_API_VERSION`

#### OpenAI

```json
{
  "providers": {
    "openAI": {
      "apiKey": "<your-openai-api-key>",
      "model": "gpt-4o-mini"
    }
  }
}
```

**Environment Variables Alternative:**
- `OPENAI_API_KEY`
- `OPENAI_MODEL`

#### GitHub Models

```json
{
  "providers": {
    "githubModels": {
      "token": "<your-github-token>",
      "model": "gpt-4o-mini"
    }
  }
}
```

**Environment Variables Alternative:**
- `GITHUB_TOKEN`
- `GITHUB_MODEL`

---

### `agents` Section

Array of agent configurations. Each agent can be individually configured.

```json
{
  "agents": [
    {
      "name": "WeatherAssistant",
      "enabled": true,
      "markdownPath": "./agents/weather-agent.md",
      "provider": "azureOpenAI",
      "overrides": {
        "temperature": 0.7,
        "maxTokens": 2000,
        "topP": 1.0,
        "frequencyPenalty": 0.0,
        "presencePenalty": 0.0
      }
    },
    {
      "name": "CodeReviewer",
      "enabled": true,
      "markdownPath": "./agents/code-reviewer.md",
      "provider": "githubModels",
      "overrides": {
        "temperature": 0.2,
        "maxTokens": 4000
      }
    }
  ]
}
```

#### Agent Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `name` | string | Unique identifier for the agent | ✅ Yes |
| `enabled` | boolean | Whether to load this agent | No (default: `true`) |
| `markdownPath` | string | Path to the agent's markdown definition file | ✅ Yes |
| `provider` | string | Override default provider for this agent | No |
| `overrides` | object | Override settings from markdown frontmatter | No |

#### Override Options

| Property | Type | Description |
|----------|------|-------------|
| `temperature` | number | Controls randomness (0.0 - 2.0) |
| `maxTokens` | number | Maximum response length |
| `topP` | number | Nucleus sampling threshold (0.0 - 1.0) |
| `frequencyPenalty` | number | Penalty for token frequency (-2.0 - 2.0) |
| `presencePenalty` | number | Penalty for token presence (-2.0 - 2.0) |
| `model` | string | Override model for this agent |

---

## Example Configurations

### Minimal Configuration

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

Uses default provider settings and environment variables.

---

### Multi-Provider Setup

```json
{
  "agentFactory": {
    "agentDefinitionsPath": "./agents",
    "defaultProvider": "azureOpenAI"
  },
  "providers": {
    "azureOpenAI": {
      "endpoint": "https://myresource.openai.azure.com",
      "deploymentName": "gpt-4"
    },
    "githubModels": {
      "model": "gpt-4o-mini"
    }
  },
  "agents": [
    {
      "name": "ProductionAgent",
      "markdownPath": "./agents/production.md",
      "provider": "azureOpenAI"
    },
    {
      "name": "DevAgent",
      "markdownPath": "./agents/dev.md",
      "provider": "githubModels"
    }
  ]
}
```

---

### Development with Auto-Reload

```json
{
  "agentFactory": {
    "agentDefinitionsPath": "./agents",
    "autoReload": true,
    "enableLogging": true,
    "logLevel": "Debug"
  },
  "agents": [
    {
      "name": "TestAgent",
      "markdownPath": "./agents/test-agent.md",
      "overrides": {
        "temperature": 0.1,
        "maxTokens": 500
      }
    }
  ]
}
```

Watches for changes to markdown files and reloads agents automatically.

---

## Security Best Practices

### ✅ Recommended: Use Environment Variables

Instead of storing secrets in `appsettings.json`:

```json
{
  "providers": {
    "azureOpenAI": {
      "endpoint": "",
      "apiKey": "",
      "deploymentName": "gpt-4o-mini"
    }
  }
}
```

Then set environment variables:
```bash
# Windows
set AZURE_OPENAI_ENDPOINT=https://myresource.openai.azure.com
set AZURE_OPENAI_API_KEY=your-secret-key

# Linux/Mac
export AZURE_OPENAI_ENDPOINT=https://myresource.openai.azure.com
export AZURE_OPENAI_API_KEY=your-secret-key
```

### ✅ Use `appsettings.Development.json`

Create separate configuration for development:

```json
// appsettings.Development.json
{
  "agentFactory": {
    "enableLogging": true,
    "logLevel": "Debug"
  }
}
```

### ✅ Add to `.gitignore`

```
# Don't commit secrets
appsettings.Local.json
appsettings.*.Local.json
```

---

## Loading Configuration in Code

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var agentFactoryConfig = configuration.GetSection("agentFactory");
var agentsConfig = configuration.GetSection("agents");
```

---

## Directory Structure Example

```
YourProject/
├── appsettings.json
├── appsettings.Development.json
├── agents/
│   ├── weather-agent.md
│   ├── code-reviewer.md
│   └── translator.md
├── generated/           # Optional output directory
└── Program.cs
```

---

## Next Steps

1. Copy `appsettings.json` to your project
2. Configure your provider credentials (preferably via environment variables)
3. Create agent markdown files in the `agentDefinitionsPath` directory
4. Update the `agents` array to reference your markdown files
5. Run the application to generate agents!

---

**Last Updated**: 2026-01-30
