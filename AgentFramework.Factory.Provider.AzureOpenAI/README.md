# AgentFramework.Factory.Provider.AzureOpenAI

Azure OpenAI provider package for AgentFramework.Factory.

## Installation

```bash
dotnet add package AgentFramework.Factory.Provider.AzureOpenAI
```

## Usage

### Configuration

Add Azure OpenAI configuration to your `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o-mini",
    "ApiVersion": "2024-08-01-preview"
  }
}
```

### Registration

Register the provider in your application:

```csharp
using AgentFramework.Factory.Provider.AzureOpenAI.Extensions;

// Using configuration
services.AddAzureOpenAIProvider(configuration);

// Or using action
services.AddAzureOpenAIProvider(options =>
{
    options.Endpoint = "https://your-resource.openai.azure.com";
    options.DeploymentName = "gpt-4o-mini";
    // ApiKey is optional - uses DefaultAzureCredential if not provided
});
```

## Features

- **Azure Identity Support**: Supports both API key and Azure credentials (Managed Identity, Azure CLI)
- **Configurable Deployment**: Specify deployment name and API version
- **Chain of Responsibility**: Automatically falls back to other providers if configured

## License

MIT
