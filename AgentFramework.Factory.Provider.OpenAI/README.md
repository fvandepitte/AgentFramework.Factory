# AgentFramework.Factory.Provider.OpenAI

OpenAI provider package for AgentFramework.Factory.

## Installation

```bash
dotnet add package AgentFramework.Factory.Provider.OpenAI
```

## Usage

### Configuration

Add OpenAI configuration to your `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key",
    "Model": "gpt-4o-mini"
  }
}
```

### Registration

Register the provider in your application:

```csharp
using AgentFramework.Factory.Provider.OpenAI.Extensions;

// Using configuration
services.AddOpenAIProvider(configuration);

// Or using action
services.AddOpenAIProvider(options =>
{
    options.ApiKey = "sk-your-api-key";
    options.Model = "gpt-4o-mini";
});
```

## Supported Models

- gpt-4o
- gpt-4o-mini
- gpt-4-turbo
- gpt-4
- gpt-3.5-turbo
- o1
- o1-mini
- o1-preview

## Features

- **Model Validation**: Validates model names against known OpenAI models
- **Chain of Responsibility**: Automatically falls back to other providers if configured

## License

MIT
