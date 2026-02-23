# AgentFramework.Factory – Declarative Agents for .NET

The **AgentFramework.Factory** library lets developers define and run AI agents in .NET using plain Markdown files.  It bridges the gap between declarative agent definitions (familiar from [GitHub Copilot’s `agents.md`](https://copilot.github.com/docs)) and the Microsoft Agent Framework, which currently only supports declarative workflows.  With this library you can author agents in a human‑readable format, plug in different Large Language Model (LLM) providers, and run conversations without writing boilerplate code.

## Why use AgentFramework.Factory?

- **Markdown‑based definitions** – Agents are described in `.md` files with YAML front matter for metadata (name, description, model, tools, temperature, etc.) and Markdown content for persona, examples and boundaries.  This format is easy to read, version and modify.
- **Provider‑agnostic** – A chain‑of‑responsibility pattern automatically selects the right LLM provider (Azure OpenAI, OpenAI, GitHub Models or custom providers) based on the model requested.  You can register multiple providers and the library will fall back to the next one if the previous cannot serve a request.
- **Fluent builder API** – Services are registered via extension methods that return an `IAgentFrameworkBuilder`, allowing you to configure agent definitions, providers, tool providers and options with fluent calls.
- **Extensible tool system** – Custom tools are supplied via `IToolProvider` implementations; they can be wired into your agents and invoked by the LLM.  Built‑in examples include simple functions and external API calls.
- **Flexible configuration** – You can specify agent definition paths, file patterns, default provider and other options via JSON or code‑based configuration.
- **NuGet‑ready and open source** – The library is packaged for .NET developers and includes optional provider packages for Azure OpenAI, OpenAI and GitHub Models.

```bash
dotnet add package AgentFramework.Factory
```

```csharp
using AgentFramework.Factory.Extensions;

services.AddAgentFramework(configuration)
    .AddMarkdownAgents(options => options.AgentDefinitionsPath = "./agents");

var factory = serviceProvider.GetRequiredService<IMarkdownAgentFactory>();
var agent = factory.LoadAgentFromFile("./agents/my-agent.md");
```

👉 See [AgentFramework.Factory/USAGE.md](AgentFramework.Factory/USAGE.md) for complete usage examples.

## 📦 Package Releases

This repository uses automated versioning and package publishing via GitHub Actions. When a new version tag is pushed, the workflow automatically:

- ✅ Builds all library projects
- ✅ Creates NuGet packages with the tag version
- ✅ Publishes packages to NuGet.org
- ✅ Creates a GitHub release with attached artifacts

### Releasing a New Version (Recommended Method)

The easiest way to create a release is via the automated tag creation workflow:

1. Go to **Actions** → **Create Release Tag** → **Run workflow**
2. Select version bump type: **patch**, **minor**, or **major**
3. Optionally specify a custom version (or leave empty to auto-increment)
4. Click **Run workflow** - the tag is created and the release is published automatically!

**Version bump types:**
- **Patch** (v1.0.0 → v1.0.1): Bug fixes
- **Minor** (v1.0.0 → v1.1.0): New features  
- **Major** (v1.0.0 → v2.0.0): Breaking changes

### Manual Tag Creation

You can also create tags manually via git:

```bash
# Major version (breaking changes): v1.0.0 → v2.0.0
git tag v2.0.0
git push origin v2.0.0

# Minor version (new features): v1.0.0 → v1.1.0
git tag v1.1.0
git push origin v1.1.0

# Patch version (bug fixes): v1.0.0 → v1.0.1
git tag v1.0.1
git push origin v1.0.1
```

The workflow will automatically:
1. Extract the version from the tag (e.g., `v1.2.3` → `1.2.3`)
2. Build all projects with that version
3. Create NuGet packages
4. Upload packages as GitHub release artifacts
5. Publish to NuGet.org (requires `NUGET_API_KEY` secret)

### Available Packages

- `AgentFramework.Factory` - Core library
- `AgentFramework.Factory.Provider.AzureOpenAI` - Azure OpenAI provider
- `AgentFramework.Factory.Provider.OpenAI` - OpenAI provider
- `AgentFramework.Factory.Provider.GitHubModels` - GitHub Models provider

For detailed release instructions and troubleshooting, see [RELEASE_WORKFLOW.md](RELEASE_WORKFLOW.md).

## Project Structure

This repository contains:

- **[AgentFramework.Factory/](AgentFramework.Factory/)** - Core reusable library (⭐ **Use this in your projects**)
- **[AgentFramework.Factory.TestConsole/](AgentFramework.Factory.TestConsole/)** - Reference CLI implementation
- **[librarystructure.md](librarystructure.md)** - Architecture and design documentation

## ✨ Key Features

- 📝 **Markdown-based agent definitions** - Define agents using markdown with YAML frontmatter
- 🔌 **Provider abstraction** - Support for Azure OpenAI, OpenAI, GitHub Models, and custom providers
- 🛠️ **Extensible tool system** - Add custom tools via `IToolProvider` interface
- ⚙️ **Flexible configuration** - Layered configuration with per-agent overrides
- 🔗 **Chain of Responsibility** - Automatic provider fallback and model routing
- 🏗️ **Fluent builder API** - Clean, intuitive service registration
- 📦 **NuGet ready** - Packaged for easy distribution and consumption
- 🧪 **Well-documented** - Comprehensive docs, examples, and XML comments

## 💡 Why This Exists

The Microsoft Agent Framework has different levels of declarative support:

- ✅ **Python** - Full declarative agent support via YAML
- ✅ **.NET** - Declarative *workflows*, but agents must be code-first
- ✅ **GitHub Copilot** - Uses markdown files for agent definitions

**AgentFramework.Factory** bridges this gap, bringing markdown-based declarative agent definitions to .NET, inspired by GitHub Copilot's approach.

## Project Goal

Create an extension for the **Microsoft Agent Framework** that enables agent generation using **Markdown files**, similar to how GitHub Copilot agents are defined. This approach provides a declarative, human-readable configuration format that separates agent definitions from implementation code.

---

## Key Inspiration: GitHub Copilot Agent Format

GitHub Copilot uses markdown files (typically `.github/agents/*.md` or `agents.md`) with **YAML frontmatter** to define custom agents. This format includes:

### YAML Frontmatter Properties
```yaml
---
name: agent_name
description: Expert in specific domain
tools: ["read", "edit", "search"]
target: github-copilot
infer: true
metadata:
  team: engineering
  owner: username
---
```

### Markdown Body Structure
- **Persona and Responsibilities**: Define the agent's role and expertise
- **Code/Documentation Examples**: Provide concrete examples for the agent to follow
- **Boundaries**: Explicit restrictions (e.g., "Never modify files in /vendor")
- **Commands**: Specific workflows the agent should execute
- **Tech Stack**: Technologies and versions the agent should know

**Reference Resources:**
- [How to write a great agents.md](https://github.blog/ai-and-ml/github-copilot/how-to-write-a-great-agents-md-lessons-from-over-2500-repositories/)
- [Custom agents configuration - GitHub Docs](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
- [Custom Agent Files | GitHub Agentic Workflows](https://githubnext.github.io/gh-aw/reference/custom-agents/)
- [Build a Custom Copilot @test-agent](https://aize.dev/546/how-to-build-a-custom-copilot-test-agent-with-agents-md/)

---

## Microsoft Agent Framework Context

The Microsoft Agent Framework supports both **code-first** and **declarative (YAML-based)** agent definitions for .NET and Python.

### Declarative YAML Configuration Example
```yaml
# weather_agent.yaml
agent:
  name: "WeatherAgent"
  description: "Answers questions about the weather using live data."
  provider: "AzureOpenAI"
  instructions: |
    You are a weather expert. Use the 'get_weather' tool to answer questions.
  tools:
    - name: get_weather
      description: "Gets current weather for a given location."
      parameters:
        - name: location
          type: string
          required: true
```

### .NET Code-First Example
```csharp
using Microsoft.Agents.AI;

var agent = new Agent(
    name: "Travel Agent",
    instructions: "Help users plan trips by answering questions about flights and hotels.",
    provider: new OpenAIProvider("<api-key>", model: "gpt-4"),
    tools: new List<ITool>
    {
        new Tool(
            name: "SearchFlights",
            description: "Finds flights between cities",
            function: SearchFlightsFunction)
    }
);
```

**Reference Resources:**
- [Microsoft Agent Framework Samples - GitHub](https://github.com/microsoft/Agent-Framework-Samples)
- [Agent Framework documentation | Microsoft Learn](https://learn.microsoft.com/en-us/agent-framework/)
- [Agent Framework Tutorials | Microsoft Learn](https://learn.microsoft.com/en-us/agent-framework/tutorials/overview)
- [Exploring Microsoft Agent Framework - Basic Agent (.NET)](https://microsoft.github.io/ai-agents-for-beginners/02-explore-agentic-frameworks/code_samples/02-dotnet-agent-framework.html)

---

## Recommended Markdown Parsing Libraries for .NET

### 1. **Markdig** (Primary Recommendation)
- **Fast**, CommonMark compliant, highly extensible
- Supports **GitHub Flavored Markdown**
- Built-in **YAML frontmatter extension** (`.UseYamlFrontMatter()`)
- 20+ extensions: tables, footnotes, task lists, strikethrough, etc.
- **AST (Abstract Syntax Tree)** support for advanced manipulation

**NuGet:** `Markdig`

**Example:**
```csharp
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

var pipeline = new MarkdownPipelineBuilder()
    .UseYamlFrontMatter()
    .Build();

var md = File.ReadAllText("agent.md");
var doc = Markdown.Parse(md, pipeline);

var yamlBlock = doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
var yaml = yamlBlock?.Lines.ToString();
```

**Resources:**
- [GitHub - xoofx/markdig](https://github.com/xoofx/markdig)
- [NuGet Gallery | Markdig](https://www.nuget.org/packages/Markdig)
- [How to edit Markdown files in C# with Markdig](https://www.luisllamas.es/en/csharp-markdig/)
- [A Crash Course in Markdig](https://johnh.co/blog/a-crash-course-in-markdig)

---

### 2. **YamlDotNet**
- **Robust YAML serialization/deserialization** for .NET
- Converts YAML to strongly-typed C# objects
- Pairs perfectly with Markdig for frontmatter extraction

**NuGet:** `YamlDotNet`

**Example:**
```csharp
using YamlDotNet.Serialization;

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
