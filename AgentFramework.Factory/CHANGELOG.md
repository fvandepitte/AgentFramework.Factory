# Changelog

All notable changes to the AgentFramework.Factory project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-02

### Added - Core Library Restructure

#### Abstractions
- `IMarkdownAgentFactory` - Factory for loading agents from markdown files
- `ILoadedAgent` - Interface for loaded agent representation
- `IProviderHandler` - Provider handler with Chain of Responsibility pattern
- `IToolProvider` - Tool provider abstraction
- `IAgentRunner` - Interface for executing conversations with agents
- `IAgentBuilder` - Fluent API for programmatic agent creation
- `IAgentRepository` - Repository pattern for loading/saving agents from various sources

#### Models
- `LoadedAgent` - Runtime agent representation implementing ILoadedAgent
- `AgentMetadata` - YAML frontmatter metadata with YamlDotNet attributes
- `AgentValidationResult` - Structured validation results for markdown parsing

#### Configuration
- `AgentFactoryConfiguration` - Core factory configuration
- `AgentConfigurationEntry` - Per-agent configuration entries
- `AgentOverrides` - Configuration override settings
- `ToolsConfiguration` - Tool management configuration
- `ToolDefinition` - Registered tool definitions

#### Services
- `MarkdownAgentFactory` - Core implementation of markdown parsing and agent loading
- `BaseProviderHandler` - Base implementation of IProviderHandler with Chain of Responsibility

#### Extensions
- `ServiceCollectionExtensions` - Dependency injection extensions
- `IAgentFrameworkBuilder` - Fluent builder API for service registration
- `AgentFrameworkBuilder` - Default builder implementation

#### Exceptions
- `AgentLoadException` - Thrown when agent loading fails
- `ProviderNotFoundException` - Thrown when provider cannot be found
- `ToolResolutionException` - Thrown when tool resolution fails

#### Documentation
- `README.md` - Core library overview and quick start
- `USAGE.md` - Comprehensive usage examples and patterns

#### NuGet Package
- Package metadata configured in .csproj
- Package ID: `AgentFramework.Factory`
- Target: .NET 10.0
- License: MIT

### Changed
- Enhanced .csproj with NuGet package metadata
- Added necessary NuGet dependencies (Markdig, YamlDotNet, Microsoft.Extensions.*)

### Technical Details

#### Dependencies Added
- `Microsoft.Extensions.Configuration.Abstractions` (10.0.2)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.2)
- `Microsoft.Extensions.Logging.Abstractions` (10.0.2)
- `Microsoft.Extensions.Options` (10.0.2)
- `Microsoft.Extensions.Options.ConfigurationExtensions` (10.0.2)
- `Markdig` (0.44.0)
- `YamlDotNet` (16.3.0)

#### Architecture
- Followed librarystructure.md specifications
- Implemented folder structure as documented
- Added all missing core abstractions
- Separated concerns between core library and test console
- Maintained backward compatibility with existing TestConsole

### Migration Notes

For users of AgentFramework.Factory.TestConsole:
- No breaking changes to TestConsole functionality
- Core library can now be used independently
- TestConsole continues to work as reference implementation

For new users:
- Install `AgentFramework.Factory` NuGet package
- Use fluent API for service registration
- Implement `IProviderHandler` for custom providers
- Implement `IToolProvider` for custom tools
