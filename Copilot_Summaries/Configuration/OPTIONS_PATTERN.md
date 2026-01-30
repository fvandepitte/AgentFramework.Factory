# Options Pattern Implementation

## Overview

The application now uses the **Microsoft.Extensions.Options** pattern for dependency injection of configuration models. This provides strongly-typed configuration with the ability to reload settings at runtime and proper separation of concerns.

---

## What Changed

### ✅ Configuration Registration

Configuration models are now registered using the Options pattern in [ServiceCollectionExtensions.cs](./Infrastructure/ServiceCollectionExtensions.cs):

```csharp
// Register IConfiguration
services.AddSingleton<IConfiguration>(configuration);

// Register configuration models using Options pattern
services.Configure<AppConfiguration>(configuration);
services.Configure<AgentFactoryConfiguration>(configuration.GetSection("agentFactory"));
services.Configure<ProvidersConfiguration>(configuration.GetSection("providers"));
services.Configure<AzureOpenAIConfiguration>(configuration.GetSection("providers:azureOpenAI"));
services.Configure<OpenAIConfiguration>(configuration.GetSection("providers:openAI"));
services.Configure<GitHubModelsConfiguration>(configuration.GetSection("providers:githubModels"));
```

### ✅ Service Updates

All services now receive configuration via `IOptions<T>` instead of direct `AppConfiguration`:

**Before:**
```csharp
public class AgentFactory
{
    public AgentFactory(AppConfiguration configuration, ...)
    {
        this.configuration = configuration;
    }
}
```

**After:**
```csharp
public class AgentFactory
{
    public AgentFactory(IOptions<AppConfiguration> configOptions, ...)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        this.configuration = configOptions.Value;
    }
}
```

---

## Updated Files

### Infrastructure
- ✅ [ServiceCollectionExtensions.cs](./Infrastructure/ServiceCollectionExtensions.cs) - Configuration registration

### Factories
- ✅ [AgentFactory.cs](./Services/Factories/AgentFactory.cs) - Uses `IOptions<AppConfiguration>`
- ✅ [ProviderFactory.cs](./Services/Factories/ProviderFactory.cs) - Uses `IOptions<AppConfiguration>`
- ✅ [MarkdownAgentFactory.cs](./Services/Factories/MarkdownAgentFactory.cs) - Uses `IOptions<AppConfiguration>`

### Providers
- ✅ [BaseProviderHandler.cs](./Services/Providers/BaseProviderHandler.cs) - Uses `IOptions<AppConfiguration>`
- ✅ [AzureOpenAIProviderHandler.cs](./Services/Providers/AzureOpenAIProviderHandler.cs) - Inherits from updated base
- ✅ [OpenAIProviderHandler.cs](./Services/Providers/OpenAIProviderHandler.cs) - Inherits from updated base
- ✅ [GitHubModelsProviderHandler.cs](./Services/Providers/GitHubModelsProviderHandler.cs) - Inherits from updated base

---

## Benefits

### 🔧 Strongly-Typed Configuration
All configuration sections are strongly-typed and validated at startup:
```csharp
IOptions<AgentFactoryConfiguration> agentFactoryOptions
IOptions<ProvidersConfiguration> providersOptions
IOptions<AzureOpenAIConfiguration> azureOptions
```

### 🔄 Runtime Reload Support
Configuration can be reloaded without restarting the application (when using `IOptionsSnapshot<T>` or `IOptionsMonitor<T>`).

### 🎯 Dependency Injection Best Practices
- Services don't need to know about `IConfiguration` directly
- Configuration is injected where needed
- Easy to mock for unit testing

### 📦 Separation of Concerns
Configuration models are clearly separated:
- **Root**: `AppConfiguration` - Full application settings
- **Sections**: `AgentFactoryConfiguration`, `ProvidersConfiguration` - Specific areas
- **Provider-specific**: `AzureOpenAIConfiguration`, `OpenAIConfiguration`, etc.

---

## Usage Examples

### Accessing Full Configuration
```csharp
public class MyService
{
    private readonly AppConfiguration config;
    
    public MyService(IOptions<AppConfiguration> configOptions)
    {
        config = configOptions.Value;
    }
}
```

### Accessing Specific Section
```csharp
public class MyProviderService
{
    private readonly ProvidersConfiguration config;
    
    public MyProviderService(IOptions<ProvidersConfiguration> configOptions)
    {
        config = configOptions.Value;
    }
}
```

### Accessing Nested Configuration
```csharp
public class AzureService
{
    private readonly AzureOpenAIConfiguration config;
    
    public AzureService(IOptions<AzureOpenAIConfiguration> configOptions)
    {
        config = configOptions.Value;
    }
}
```

---

## Testing

### Unit Testing with Options
```csharp
var config = new AppConfiguration
{
    AgentFactory = new AgentFactoryConfiguration
    {
        DefaultProvider = "azureOpenAI"
    }
};

var options = Options.Create(config);
var service = new AgentFactory(options, ...);
```

### Mocking Options
```csharp
var mockOptions = new Mock<IOptions<AppConfiguration>>();
mockOptions.Setup(o => o.Value).Returns(new AppConfiguration { ... });

var service = new AgentFactory(mockOptions.Object, ...);
```

---

## Configuration Hierarchy

```
AppConfiguration (IOptions<AppConfiguration>)
├── AgentFactory (IOptions<AgentFactoryConfiguration>)
│   ├── AgentDefinitionsPath
│   ├── DefaultProvider
│   ├── ProviderChain
│   └── EnableLogging
├── Providers (IOptions<ProvidersConfiguration>)
│   ├── AzureOpenAI (IOptions<AzureOpenAIConfiguration>)
│   │   ├── Endpoint
│   │   ├── ApiKey
│   │   └── DeploymentName
│   ├── OpenAI (IOptions<OpenAIConfiguration>)
│   │   ├── ApiKey
│   │   └── Model
│   └── GitHubModels (IOptions<GitHubModelsConfiguration>)
│       ├── Token
│       └── Model
└── Agents (List<AgentConfigurationEntry>)
```

---

## NuGet Packages Added

- ✅ `Microsoft.Extensions.Options` (10.0.2)
- ✅ `Microsoft.Extensions.Options.ConfigurationExtensions` (10.0.2)

---

## Advanced Scenarios

### Using IOptionsSnapshot for Reload Support
If you need configuration to reload when `appsettings.json` changes:

```csharp
public class MyService
{
    private readonly IOptionsSnapshot<AppConfiguration> configSnapshot;
    
    public MyService(IOptionsSnapshot<AppConfiguration> configSnapshot)
    {
        this.configSnapshot = configSnapshot;
    }
    
    public void DoSomething()
    {
        // Gets latest configuration value
        var currentConfig = configSnapshot.Value;
    }
}
```

### Using IOptionsMonitor for Change Notifications
If you need to react to configuration changes:

```csharp
public class MyService
{
    private readonly IOptionsMonitor<AppConfiguration> configMonitor;
    
    public MyService(IOptionsMonitor<AppConfiguration> configMonitor)
    {
        this.configMonitor = configMonitor;
        
        // React to changes
        configMonitor.OnChange(config =>
        {
            Console.WriteLine("Configuration changed!");
        });
    }
    
    public void DoSomething()
    {
        // Gets latest configuration value
        var currentConfig = configMonitor.CurrentValue;
    }
}
```

### Validation with IValidateOptions
Add validation for configuration:

```csharp
public class AppConfigurationValidator : IValidateOptions<AppConfiguration>
{
    public ValidateOptionsResult Validate(string name, AppConfiguration options)
    {
        if (string.IsNullOrEmpty(options.AgentFactory.DefaultProvider))
        {
            return ValidateOptionsResult.Fail("DefaultProvider is required");
        }
        
        return ValidateOptionsResult.Success;
    }
}

// Register in DI
services.AddSingleton<IValidateOptions<AppConfiguration>, AppConfigurationValidator>();
```

---

## Next Steps

Potential enhancements:
- Add configuration validation using `IValidateOptions<T>`
- Implement `IOptionsSnapshot<T>` for runtime reload scenarios
- Add options post-configuration using `PostConfigure<T>()`
- Create named options for multiple provider instances

---

**Last Updated**: 2026-01-30  
**Status**: ✅ Fully implemented and tested

