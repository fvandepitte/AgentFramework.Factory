# Namespace Structure

- AgentFramework.Factory
  - AgentFramework.Factory.Provider
    - AgentFramework.Factory.Provider.AzureOpenAI
    - AgentFramework.Factory.Provider.OpenAI
    - AgentFramework.Factory.Provider.GitHubModels

# Modules

## AgentFramework.Factory

Module contains `AgentFactory`, `MarkdownAgentFactory`, `ToolFactory` and a `ServiceCollectionExtensions`

Make sure everything is in its own namespace.

### Folder Structure

```
AgentFramework.Factory/
├── Abstractions/
│   ├── ILoadedAgent.cs
│   ├── IMarkdownAgentFactory.cs
│   ├── IProviderHandler.cs
│   ├── IToolProvider.cs
│   ├── IAgentRunner.cs          # Execute conversations with loaded agents
│   ├── IAgentBuilder.cs         # Fluent API for programmatic agent creation
│   └── IAgentRepository.cs      # Load/save agents from various sources
├── Services/
│   ├── MarkdownAgentFactory.cs
│   ├── ProviderFactory.cs
│   ├── ToolFactory.cs
│   └── BaseProviderHandler.cs
├── Configuration/
│   ├── AgentFactoryConfiguration.cs
│   ├── AgentConfigurationEntry.cs
│   └── ToolsConfiguration.cs
├── Models/
│   ├── LoadedAgent.cs
│   ├── AgentMetadata.cs
│   └── AgentValidationResult.cs  # Structured validation for markdown parsing
├── Exceptions/
│   ├── AgentLoadException.cs
│   ├── ProviderNotFoundException.cs
│   └── ToolResolutionException.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

### ServiceCollectionExtensions

- Register the factories
- Register settings related to these factories
- Provide fluent builder pattern for registration

### Fluent Registration API

```csharp
services.AddAgentFramework()
    .AddMarkdownAgents(options => options.AgentDefinitionsPath = "./agents")
    .AddProvider<AzureOpenAIProviderHandler>(options => 
    {
        options.Endpoint = "...";
        options.DeploymentName = "gpt-4o";
    })
    .AddToolProvider<LocalToolProvider>()
    .AddMcpServer("github", "https://api.githubcopilot.com/mcp/");
```

### Provider Extensibility Interface

```csharp
public interface IProviderConfiguration
{
    string ProviderName { get; }
    void Configure(IServiceCollection services, IConfiguration configuration);
}
```

---

## AgentFramework.Factory.Provider.x

Module contains specific Options, AppConfiguration extension and code for specific provider.

### Package Structure

| Package | Contents |
|---------|----------|
| `AgentFramework.Factory` | Core abstractions, base classes, DI extensions |
| `AgentFramework.Factory.Provider.AzureOpenAI` | Azure OpenAI handler + options |
| `AgentFramework.Factory.Provider.OpenAI` | OpenAI handler + options |
| `AgentFramework.Factory.Provider.GitHubModels` | GitHub Models handler + options |

### Provider Package Structure

```
AgentFramework.Factory.Provider.AzureOpenAI/
├── AzureOpenAIProviderHandler.cs
├── AzureOpenAIConfiguration.cs
├── AzureOpenAIProviderConfiguration.cs  # Implements IProviderConfiguration
└── Extensions/
    └── ServiceCollectionExtensions.cs   # .AddAzureOpenAIProvider()
```

---

## Missing Core Abstractions

| Component | Purpose |
|-----------|---------|
| **`IAgentRunner`** | Execute conversations with loaded agents |
| **`IAgentBuilder`** | Fluent API for programmatic agent creation |
| **`AgentValidationResult`** | Structured validation for markdown parsing |
| **`IAgentRepository`** | Load/save agents from various sources (files, database, etc.) |

---

## Production Features (Future)

| Feature | Why Needed |
|---------|------------|
| **Hot reload** | Watch markdown files for changes |
| **Telemetry/metrics** | Track agent usage, latency, token consumption |
| **Caching** | Cache parsed markdown, chat clients |
| **Health checks** | Verify provider connectivity |
| **Retry policies** | Resilience for transient failures |

---

## NuGet Package Metadata

Add to each `.csproj`:

```xml
<PropertyGroup>
    <PackageId>AgentFramework.Factory</PackageId>
    <Version>1.0.0</Version>
    <Authors>Frederiek Vandepitte</Authors>
    <Description>Markdown-based agent definitions for Microsoft Agent Framework</Description>
    <PackageTags>agents;ai;markdown;copilot;llm</PackageTags>
    <RepositoryUrl>https://github.com/fvandepitte/AgentFramework.Factory</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
</PropertyGroup>
```
