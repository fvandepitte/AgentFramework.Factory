# AgentFramework.Factory – Declarative Agents for .NET

The **AgentFramework.Factory** library lets developers define and run AI agents in .NET using plain Markdown files.  It bridges the gap between declarative agent definitions (familiar from [GitHub Copilot’s `agents.md`](https://copilot.github.com/docs)) and the Microsoft Agent Framework, which currently only supports declarative workflows.  With this library you can author agents in a human‑readable format, plug in different Large Language Model (LLM) providers, and run conversations without writing boilerplate code.

## Why use AgentFramework.Factory?

- **Markdown‑based definitions** – Agents are described in `.md` files with YAML front matter for metadata (name, description, model, tools, temperature, etc.) and Markdown content for persona, examples and boundaries.  This format is easy to read, version and modify.
- **Provider‑agnostic** – A chain‑of‑responsibility pattern automatically selects the right LLM provider (Azure OpenAI, OpenAI, GitHub Models or custom providers) based on the model requested.  You can register multiple providers and the library will fall back to the next one if the previous cannot serve a request.
- **Fluent builder API** – Services are registered via extension methods that return an `IAgentFrameworkBuilder`, allowing you to configure agent definitions, providers, tool providers and options with fluent calls.
- **Extensible tool system** – Custom tools are supplied via `IToolProvider` implementations; they can be wired into your agents and invoked by the LLM.  Built‑in examples include simple functions and external API calls.
- **Flexible configuration** – You can specify agent definition paths, file patterns, default provider and other options via JSON or code‑based configuration.
- **NuGet‑ready and open source** – The library is packaged for .NET developers and includes optional provider packages for Azure OpenAI, OpenAI and GitHub Models.

## Quick start

1. **Install the package.**  Add the core library to your project:

   ```bash
   dotnet add package AgentFramework.Factory
   ```
   To use specific LLM providers, also install the corresponding provider package (e.g. `AgentFramework.Factory.Provider.AzureOpenAI`).

2. **Register services and load agents.**  In your `Program.cs` or wherever you configure services, register the framework and your agents:

   ```csharp
   using AgentFramework.Factory;
   using AgentFramework.Factory.Provider.OpenAI;

   var services = new ServiceCollection();

   services
       // register the core framework and load Markdown agents from a folder
       .AddAgentFramework(configuration)
       .AddMarkdownAgents(options => {
           options.AgentDefinitionsPath = "./agents";     // folder containing .md agent definitions
       })
       // add an OpenAI provider (also supports Azure OpenAI or GitHub Models)
       .AddOpenAIProvider(options => {
           options.ApiKey = "<your-api-key>";
           options.DefaultModel = "gpt-4";
       })
       // optionally add tool providers
       .AddToolProvider<MyCustomToolProvider>();

   var serviceProvider = services.BuildServiceProvider();

   // get the agent factory and load an agent by file
   var factory = serviceProvider.GetRequiredService<IMarkdownAgentFactory>();
   var loadedAgent = await factory.CreateAgentByName("my-agent");
   ```


### Defining an agent in Markdown

Agents are defined in Markdown files.  The YAML front matter contains metadata such as the agent’s name, model and tools, while the Markdown body specifies the persona and examples.  A minimal agent might look like this:

```markdown
---
name: greeting-agent
description: A polite bot that greets the user.
model: gpt-4
temperature: 0.2
tools:
  - WeatherTool
---

# Persona

You are a friendly assistant who greets the user and can also tell the weather using the WeatherTool.

## Examples

- User: Hi!
  Assistant: Hello!  How can I help you today?
```

Place your `.md` files in the folder configured via `options.AgentDefinitionsPath`.  The framework will scan the directory and load all definitions.

## Architecture overview

The library is organised into several abstractions to keep your code decoupled and extensible:

1. **`IMarkdownAgentFactory`** – Main entry point to load agents from Markdown files.  It parses YAML metadata, constructs an `ILoadedAgent`, validates configuration and wires up providers and tools
2. **`ILoadedAgent`** – Represents a parsed agent.
3. **`IProviderHandler`** – Implements the chain‑of‑responsibility pattern for LLM providers.  Each handler decides if it can serve a model; if not, it passes the request down the chain.
4. **`IToolProvider`** – Supplies custom tools that the agent can invoke during conversations.  Tools are described in YAML and implemented in code.
5. **Fluent builder** – Extension methods (`AddAgentFramework`, `AddMarkdownAgents`, `AddOpenAIProvider`, etc.) return an `IAgentFrameworkBuilder` so you can chain configuration calls fluently.

## Provider packages

The core library is provider‑agnostic.  Optional packages implement specific LLM providers and follow a common configuration pattern:

| Provider package | Configuration | Notes |
| --- | --- | --- |
| **AgentFramework.Factory.Provider.AzureOpenAI** | API key or Managed Identity, resource name and deployment name | Suitable for Azure OpenAI resources |
| **AgentFramework.Factory.Provider.OpenAI** | API key and default model | Supports chat and completion models |
| **AgentFramework.Factory.Provider.GitHub** | GitHub personal access token or default credentials | Leverages GitHub Models via GitHub Copilot infrastructure |

You can implement your own providers by inheriting from `IProviderHandler` and registering them through the builder API.  The provider options pattern ensures configuration is consistent across packages.

## Extending the framework

AgentFramework.Factory is designed to be extensible:

- **Add custom tools.**  Implement `IToolProvider` and register it with `AddToolProvider<T>()`.  Tools can expose functions, data sources or third‑party APIs to your agent.
- **Create new providers.**  Subclass `IProviderHandler` to integrate additional LLMs or custom back‑ends.  Register them with `AddProviderHandler<T>()`.  The chain‑of‑responsibility ensures fallback when a provider cannot handle a model.
- **Watch for file changes.**  A future enhancement proposes hot‑reloading Markdown files; you can contribute or fork the library to add this feature.

## Further resources

The repository contains several additional documents that explain the design in more depth:

- **`AgentFramework.Factory/README.md`** – Describes core abstractions, interfaces and classes.
- **`EXTENSIBILITY.md`** – Shows how to extend the framework by writing custom providers and tool providers.
- **`PROVIDER_PACKAGES_SUMMARY.md`** – Lists all provider packages and their configuration.
- **`IMPLEMENTATION_SUMMARY.md`** – Summarises the motivation, architecture and potential future enhancements.

## Contributing & License

Contributions are welcome!  Please open issues or pull requests in the repository.  The project follows the [MIT License](https://opensource.org/licenses/MIT) unless otherwise noted.
