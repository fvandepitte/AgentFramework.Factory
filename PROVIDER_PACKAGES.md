# Provider Packages Guide

This guide explains how to use the separate provider packages for AgentFramework.Factory.

## Overview

AgentFramework.Factory now has three separate provider packages:

- **AgentFramework.Factory.Provider.AzureOpenAI** - Azure OpenAI integration
- **AgentFramework.Factory.Provider.OpenAI** - OpenAI integration
- **AgentFramework.Factory.Provider.GitHubModels** - GitHub Models integration

Each provider package is independently installable and can be used separately or together.

## Installation

### Install Core Library

```bash
dotnet add package AgentFramework.Factory
```

### Install Provider Packages

Install only the providers you need:

```bash
# Azure OpenAI
dotnet add package AgentFramework.Factory.Provider.AzureOpenAI

# OpenAI
dotnet add package AgentFramework.Factory.Provider.OpenAI

# GitHub Models
dotnet add package AgentFramework.Factory.Provider.GitHubModels
```

## Usage Examples

### Using Azure OpenAI Provider

```csharp
using AgentFramework.Factory.Extensions;
using AgentFramework.Factory.Provider.AzureOpenAI.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register core library
services.AddAgentFramework(configuration);

// Register Azure OpenAI provider
services.AddAzureOpenAIProvider(options =>
{
    options.Endpoint = "https://your-resource.openai.azure.com";
    options.DeploymentName = "gpt-4o-mini";
    // ApiKey is optional - uses DefaultAzureCredential if not provided
});

var serviceProvider = services.BuildServiceProvider();
```

### Using OpenAI Provider

```csharp
using AgentFramework.Factory.Extensions;
using AgentFramework.Factory.Provider.OpenAI.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register core library
services.AddAgentFramework(configuration);

// Register OpenAI provider
services.AddOpenAIProvider(options =>
{
    options.ApiKey = "sk-your-api-key";
    options.Model = "gpt-4o-mini";
});

var serviceProvider = services.BuildServiceProvider();
```

### Using GitHub Models Provider

```csharp
using AgentFramework.Factory.Extensions;
using AgentFramework.Factory.Provider.GitHubModels.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register core library
services.AddAgentFramework(configuration);

// Register GitHub Models provider
services.AddGitHubModelsProvider(options =>
{
    options.Token = "ghp_your-github-token";
    options.Model = "gpt-4o-mini";
});

var serviceProvider = services.BuildServiceProvider();
```

### Using Multiple Providers (Chain of Responsibility)

You can register multiple providers to enable automatic fallback:

```csharp
using AgentFramework.Factory.Extensions;
using AgentFramework.Factory.Provider.AzureOpenAI.Extensions;
using AgentFramework.Factory.Provider.OpenAI.Extensions;
using AgentFramework.Factory.Provider.GitHubModels.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register core library
services.AddAgentFramework(configuration);

// Register all providers - they will form a chain
services.AddAzureOpenAIProvider(configuration);
services.AddOpenAIProvider(configuration);
services.AddGitHubModelsProvider(configuration);

var serviceProvider = services.BuildServiceProvider();
```

When multiple providers are registered, the factory will automatically try each provider in order until one succeeds.

## Configuration-Based Registration

You can also register providers using configuration:

### appsettings.json

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com",
    "DeploymentName": "gpt-4o-mini",
    "ApiVersion": "2024-08-01-preview"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini"
  },
  "GitHubModels": {
    "Token": "ghp_...",
    "Model": "gpt-4o-mini"
  }
}
```

### Program.cs

```csharp
using AgentFramework.Factory.Extensions;
using AgentFramework.Factory.Provider.AzureOpenAI.Extensions;
using AgentFramework.Factory.Provider.OpenAI.Extensions;
using AgentFramework.Factory.Provider.GitHubModels.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

// Register using configuration sections
services.AddAzureOpenAIProvider(configuration.GetSection("AzureOpenAI"));
services.AddOpenAIProvider(configuration.GetSection("OpenAI"));
services.AddGitHubModelsProvider(configuration.GetSection("GitHubModels"));

var serviceProvider = services.BuildServiceProvider();
```

## Provider Extensibility

Each provider implements the `IProviderConfiguration` interface, allowing for dynamic provider registration:

```csharp
using AgentFramework.Factory.Abstractions;

// Example: Dynamically load and configure providers
var providerConfigurations = new List<IProviderConfiguration>
{
    new AzureOpenAIProviderConfiguration(),
    new OpenAIProviderConfiguration(),
    new GitHubModelsProviderConfiguration()
};

foreach (var providerConfig in providerConfigurations)
{
    providerConfig.Configure(services, configuration);
}
```

## Security Best Practices

### Never Commit API Keys

Always use user secrets or environment variables for API keys:

```bash
# Azure OpenAI
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://..."

# OpenAI
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."

# GitHub Models
dotnet user-secrets set "GitHubModels:Token" "ghp_..."
```

### Use Azure Managed Identity

For Azure OpenAI, prefer using Managed Identity instead of API keys:

```csharp
services.AddAzureOpenAIProvider(options =>
{
    options.Endpoint = "https://your-resource.openai.azure.com";
    options.DeploymentName = "gpt-4o-mini";
    // Don't set ApiKey - will use DefaultAzureCredential
});
```

## Package Dependencies

Each provider package has the following dependencies:

### Common Dependencies
- `AgentFramework.Factory` (core library)
- `Microsoft.Extensions.AI`
- `Microsoft.Extensions.AI.Abstractions`
- `Microsoft.Extensions.AI.OpenAI`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Options`

### Provider-Specific Dependencies

**AzureOpenAI:**
- `Azure.AI.OpenAI`
- `Azure.Identity`

**OpenAI & GitHubModels:**
- Uses `Microsoft.Extensions.AI.OpenAI` (no additional dependencies)

## Next Steps

- See [USAGE.md](AgentFramework.Factory/USAGE.md) for core library usage
- See individual provider README files for provider-specific details
- See [librarystructure.md](librarystructure.md) for architecture details

## License

MIT
