# AgentFramework.Factory.Provider.GitHubModels

GitHub Models provider package for AgentFramework.Factory.

## Installation

```bash
dotnet add package AgentFramework.Factory.Provider.GitHubModels
```

## Usage

### Configuration

Add GitHub Models configuration to your `appsettings.json`:

```json
{
  "GitHubModels": {
    "Token": "ghp_your-github-token",
    "Model": "gpt-4o-mini"
  }
}
```

### Registration

Register the provider in your application:

```csharp
using AgentFramework.Factory.Provider.GitHubModels.Extensions;

// Using configuration
services.AddGitHubModelsProvider(configuration);

// Or using action
services.AddGitHubModelsProvider(options =>
{
    options.Token = "ghp_your-github-token";
    options.Model = "gpt-4o-mini";
});
```

## Supported Models

### OpenAI Models
- gpt-4o, gpt-4o-mini
- gpt-4-turbo, gpt-4
- gpt-3.5-turbo
- o1, o1-mini, o1-preview

### Open Source Models
- phi-3, phi-3.5
- llama-3, llama-3.1, llama-3.2
- mistral-large, mistral-nemo
- cohere-command-r, cohere-command-r-plus

## Features

- **Wide Model Support**: Access to both OpenAI and open-source models
- **GitHub Integration**: Uses GitHub's AI marketplace
- **Chain of Responsibility**: Automatically falls back to other providers if configured

## License

MIT
