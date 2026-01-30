# Dependency Injection Setup

The Agent Framework Markdown Factory now uses proper dependency injection using **Microsoft.Extensions.DependencyInjection** integrated with **Spectre.Console.Cli**.

## Architecture

### Service Registration

All services are registered in [ServiceCollectionExtensions.cs](./Infrastructure/ServiceCollectionExtensions.cs):

```csharp
services.AddAgentFactoryServices();
```

This registers:
- ✅ **AppConfiguration** - Singleton configuration loaded from appsettings.json
- ✅ **IProviderHandler** implementations - All provider handlers (Azure OpenAI, OpenAI, GitHub Models)
- ✅ **MarkdownAgentFactory** - Singleton factory for loading agents from markdown
- ✅ **ProviderFactory** - Singleton factory with Chain of Responsibility pattern (receives all handlers)
- ✅ **AgentFactory** - Singleton factory for creating agents

### Provider Handler Registration

All `IProviderHandler` implementations are registered as singletons and injected as `IEnumerable<IProviderHandler>`:

```csharp
// Register all provider handlers
services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();

// ProviderFactory receives them all
public ProviderFactory(
    AppConfiguration configuration,
    IEnumerable<IProviderHandler> providerHandlers)
{
    // Builds the chain from all registered handlers
}
```

**Benefits:**
- ✅ Easy to add new providers - just register another `IProviderHandler`
- ✅ No need to modify ProviderFactory when adding providers
- ✅ Providers are resolved from DI container
- ✅ Better testability - can mock or replace handlers

### Configuration Loading

Configuration is loaded with the following priority:

1. **appsettings.json** - Base configuration
2. **appsettings.{Environment}.json** - Environment-specific overrides
3. **Environment Variables** - Override via environment
4. **User Secrets** - Development secrets (not committed)

```csharp
var configuration = BuildConfiguration(configFilePath);
var appConfig = new AppConfiguration();
configuration.Bind(appConfig);
services.AddSingleton(appConfig);
```

### Spectre.Console.Cli Integration

The DI container is integrated with Spectre.Console.Cli using:

**TypeRegistrar** - Adapts IServiceCollection to Spectre's ITypeRegistrar
**TypeResolver** - Resolves services from the IServiceProvider

```csharp
var services = new ServiceCollection();
services.AddAgentFactoryServices();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);
```

## Command Constructor Injection

All commands receive their dependencies via constructor injection:

### Example: ListCommand

```csharp
public class ListCommand : Command<ListCommand.Settings>
{
    private readonly MarkdownAgentFactory _factory;

    public ListCommand(MarkdownAgentFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var agents = _factory.LoadAgentsFromConfiguration();
        // ...
    }
}
```

### Example: TestAgentCommand (Multiple Dependencies)

```csharp
public class TestAgentCommand : Command<TestAgentCommand.Settings>
{
    private readonly AgentFactory _agentFactory;
    private readonly AppConfiguration _config;

    public TestAgentCommand(AgentFactory agentFactory, AppConfiguration config)
    {
        _agentFactory = agentFactory;
        _config = config;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var agents = _config.Agents.Where(a => a.Enabled).ToList();
        var agent = _agentFactory.CreateAgentByName(agentName);
        // ...
    }
}
```

## Service Lifetimes

All services use **Singleton** lifetime:

```csharp
services.AddSingleton<MarkdownAgentFactory>();
services.AddSingleton<ProviderFactory>();
services.AddSingleton<AgentFactory>();
```

**Why Singleton?**
- Configuration is loaded once at startup
- Factories are stateless and thread-safe
- Provider chain is built once and reused
- Improves performance (no repeated instantiation)

## Configuration File Handling

### Default Configuration

By default, `appsettings.json` is loaded from the current directory:

```bash
dotnet run -- list
```

### Custom Configuration (Future)

To support custom configuration files, modify the extension method:

```csharp
services.AddAgentFactoryServices(configFilePath: "custom.json");
```

Or use environment variables:

```bash
export AgentFactory__DefaultProvider="openAI"
dotnet run -- list
```

## Environment-Specific Configuration

### Development

```bash
export DOTNET_ENVIRONMENT=Development
dotnet run -- list
```

Loads:
1. `appsettings.json`
2. `appsettings.Development.json` (overrides)
3. User secrets (if configured)

### Production

```bash
export DOTNET_ENVIRONMENT=Production
dotnet run -- list
```

Loads:
1. `appsettings.json`
2. `appsettings.Production.json` (overrides)
3. Environment variables

## User Secrets

The project is configured with user secrets for sensitive data:

```xml
<UserSecretsId>551fbd54-cd91-453e-a5af-ea32eb3b509c</UserSecretsId>
```

### Set a Secret

```bash
dotnet user-secrets set "providers:azureOpenAI:apiKey" "your-secret-key"
```

### List Secrets

```bash
dotnet user-secrets list
```

### Remove a Secret

```bash
dotnet user-secrets remove "providers:azureOpenAI:apiKey"
```

## Testing with DI

### Unit Testing Commands

Mock the dependencies in your tests:

```csharp
[Fact]
public void ListCommand_ShouldLoadAgents()
{
    // Arrange
    var mockConfig = new AppConfiguration();
    var mockFactory = new Mock<MarkdownAgentFactory>();
    var command = new ListCommand(mockFactory.Object);
    
    // Act
    var result = command.Execute(context, settings);
    
    // Assert
    Assert.Equal(0, result);
}
```

### Integration Testing

Create a test service collection:

```csharp
var services = new ServiceCollection();
services.AddAgentFactoryServices("test-appsettings.json");
var provider = services.BuildServiceProvider();

var factory = provider.GetRequiredService<MarkdownAgentFactory>();
var agents = factory.LoadAgentsFromConfiguration();
```

## Benefits

✅ **Testability** - Easy to mock dependencies
✅ **Maintainability** - Clear separation of concerns
✅ **Flexibility** - Easy to swap implementations
✅ **Configuration** - Centralized configuration management
✅ **Scalability** - Easy to add new services
✅ **Best Practices** - Follows .NET dependency injection patterns

## File Structure

```
Infrastructure/
├── TypeRegistrar.cs              # Spectre.Console.Cli DI adapter
└── ServiceCollectionExtensions.cs # Service registration

Services/
├── Configuration.cs               # Configuration models
├── MarkdownAgentFactory.cs        # Injected into commands
├── ProviderFactory.cs             # Injected into AgentFactory
└── AgentFactory.cs                # Injected into commands

Commands/
├── ListCommand.cs                 # Uses MarkdownAgentFactory
├── ShowCommand.cs                 # Uses MarkdownAgentFactory
├── ReadTestCommand.cs             # Uses MarkdownAgentFactory
├── InteractiveCommand.cs          # Uses MarkdownAgentFactory
└── TestAgentCommand.cs            # Uses AgentFactory + AppConfiguration
```

## Adding New Services

### Adding a New Provider Handler

1. **Create the provider handler class**:

```csharp
public class AnthropicProviderHandler : BaseProviderHandler
{
    public AnthropicProviderHandler(AppConfiguration configuration) 
        : base(configuration)
    {
    }

    public override string ProviderName => "Anthropic";

    protected override bool CanHandleModel(string modelName)
    {
        return modelName.StartsWith("claude-");
    }

    protected override IChatClient CreateChatClient(string modelName)
    {
        // Create Anthropic client
    }
}
```

2. **Register in ServiceCollectionExtensions**:

```csharp
// Register all provider handlers
services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();
services.AddSingleton<IProviderHandler, AnthropicProviderHandler>(); // New!
```

3. **Add to provider chain in configuration** (optional):

```json
{
  "agentFactory": {
    "providerChain": ["azureOpenAI", "openAI", "anthropic", "githubModels"]
  }
}
```

That's it! The `ProviderFactory` will automatically receive and use the new handler.

### Adding Other Services

1. **Create the service class**:

```csharp
public class MyNewService
{
    private readonly AppConfiguration _config;
    
    public MyNewService(AppConfiguration config)
    {
        _config = config;
    }
}
```

2. **Register in ServiceCollectionExtensions**:

```csharp
public static IServiceCollection AddAgentFactoryServices(
    this IServiceCollection services,
    string? configFilePath = null)
{
    // ... existing registrations
    services.AddSingleton<MyNewService>();
    return services;
}
```

3. **Inject into commands**:

```csharp
public class MyCommand : Command<MyCommand.Settings>
{
    private readonly MyNewService _service;
    
    public MyCommand(MyNewService service)
    {
        _service = service;
    }
}
```

## Common Patterns

### Multiple Interface Implementations

When you have multiple implementations of the same interface (like `IProviderHandler`), register them all and inject as `IEnumerable<T>`:

```csharp
// Registration
services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();
services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();

// Injection
public class ProviderFactory
{
    public ProviderFactory(IEnumerable<IProviderHandler> handlers)
    {
        // All registered handlers are injected
        foreach (var handler in handlers)
        {
            Console.WriteLine($"Handler: {handler.ProviderName}");
        }
    }
}
```

This pattern is used for:
- ✅ Strategy pattern implementations
- ✅ Plugin architectures
- ✅ Chain of Responsibility (like our provider chain)
- ✅ Composite patterns

### Service Dependency

```csharp
public class ServiceA
{
    private readonly ServiceB _serviceB;
    
    public ServiceA(ServiceB serviceB)
    {
        _serviceB = serviceB;
    }
}
```

### Optional Dependencies

```csharp
public class MyService
{
    private readonly ILogger? _logger;
    
    public MyService(ILogger? logger = null)
    {
        _logger = logger;
    }
}
```

### Configuration Injection

```csharp
public class MyService
{
    private readonly AppConfiguration _config;
    
    public MyService(AppConfiguration config)
    {
        _config = config;
    }
    
    public void DoSomething()
    {
        var provider = _config.AgentFactory.DefaultProvider;
    }
}
```

---

**Implementation Date**: 2026-01-30  
**Pattern**: Dependency Injection with Microsoft.Extensions.DependencyInjection  
**Status**: ✅ Complete and tested

