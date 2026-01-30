# Agent Framework Markdown Factory

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

var deserializer = new DeserializerBuilder().Build();
var metadata = deserializer.Deserialize<AgentMetadata>(yaml);

public class AgentMetadata
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Tools { get; set; }
}
```

**Resources:**
- [Rendering Markdown to HTML and Parsing YAML Front Matter in C#](https://markheath.net/post/markdown-html-yaml-front-matter)
- [Extract Front Matter from Markdown file in .NET Core using Markdig](https://atashbahar.com/post/2020-06-16-extract-front-matter-in-dotnet-with-markdig)
- [Strongly-Typed Markdown for ASP.NET Core Content Apps](https://khalidabuhakmeh.com/strongly-typed-markdown-for-aspnet-core-content-apps)

---

### 3. **EPS.Extensions.YamlMarkdown**
- **Unified solution** combining Markdig + YamlDotNet
- Single class handles frontmatter extraction, deserialization, and HTML rendering
- Good for quick setup and simpler workflows

**NuGet:** `EPS.Extensions.YamlMarkdown`

**Resources:**
- [NuGet Gallery | EPS.Extensions.YamlMarkdown](https://www.nuget.org/packages/EPS.Extensions.YamlMarkdown/)

---

### 4. **Alternative Libraries** (For Comparison)

#### **Aspose.HTML for .NET**
- Enterprise-level, commercial
- Supports Markdown + many other formats (HTML, PDF, DOCX)
- Requires licensing

#### **MarkdownSharpCore**
- Lightweight, .NET 6+ fork of MarkdownSharp
- Simple Markdown-to-HTML conversion
- Lacks advanced extensions

#### **Waher.Content.Markdown**
- Extensible parser with custom renderer support
- Non-HTML export options

#### **MarkdownDeep**
- Performance-focused, legacy
- No longer actively maintained

---

## Similar Projects & Standards

### 1. **AGENTS.md**
Open standard for AI coding agents (Copilot, Cursor, Codex). Used by **60,000+ open-source projects** to provide project-specific instructions in a single markdown file.

**Resources:**
- [AGENTS.md – OpenAI Codex, GitHub Copilot & Cursor AI](https://agentsmd.io/)
- [AGENTS.md](https://agents.md/)
- [Improve your AI code output with AGENTS.md](https://www.builder.io/blog/agents-md)

---

### 2. **MAGI (Markdown for Agent Guidance & Instruction)**
Extended Markdown standard for agent workflows, designed for better **RAG (Retrieval-Augmented Generation)** and structure preservation.

**Resources:**
- [Introduction - MAGI (Markdown for AI Agents)](https://docs.magi-mda.org/introduction)

---

### 3. **AI Coding Agent Manager (andrlange/ai-coding)**
Web application (Java/Spring Boot) for managing AI agent descriptions in Markdown. Supports organizing, editing, searching, and previewing agent configurations.

**Resources:**
- [GitHub - andrlange/ai-coding](https://github.com/andrlange/ai-coding)

---

### 4. **TaskMaster & Markdown-Based Workflows**
Uses structured Markdown as Project Requirement Documents (PRDs) that AI agents parse into actionable tasks.

**Resources:**
- [Markdown as AI Interface in Modern Development](https://llmtuts.com/tutorials/makdown-ai-dev-workflow/index.html)

---

### 5. **Markdown-First Documentation Systems**
Systems like **Mintlify**, **Fumadocs**, and **Lingo.dev** serve content directly as Markdown to AI agents via content negotiation.

**Resources:**
- [How to serve Markdown to AI agents](https://dev.to/lingodotdev/how-to-serve-markdown-to-ai-agents-making-your-docs-more-ai-friendly-4pdn)

---

### 6. **Other Agent Frameworks**
While not Markdown-focused, frameworks like **LangGraph**, **CrewAI**, and **Qwen-Agent** often interface with Markdown documentation for RAG and multi-agent orchestration.

**Resources:**
- [Top 10 AI Agent Projects to Build in 2026](https://www.datacamp.com/blog/top-ai-agent-projects)
- [Top 18 Open Source AI Agent Projects](https://www.nocobase.com/en/blog/github-open-source-ai-agent-projects)

---

## Key Finding: MAF Declarative Support Status

**Important Discovery (2026-01-30):**

After investigating the Microsoft Agent Framework's declarative support:

### ✅ What Works in .NET
- **Declarative Workflows**: The `Microsoft.Agents.AI.Workflows.Declarative` package supports YAML-based **workflow orchestration** (multi-agent, sequential, conditional, concurrent patterns)
- **Code-First Agents**: Fully supported and well-documented
- **Markdown Parsing**: Successfully tested with **Markdig + YamlDotNet** - can parse YAML frontmatter and markdown body

### ❓ What's Limited
- **Declarative Individual Agents**: Python has `agent-framework-declarative` package for YAML agent definitions, but .NET support is primarily workflow-focused
- The .NET framework expects agents to be created programmatically, then orchestrated via declarative workflows

### 💡 Our Opportunity
This means our **Markdown-to-Agent factory** fills a real gap in the .NET ecosystem:
- GitHub Copilot uses markdown for agents ✅
- Python MAF supports declarative agents ✅
- .NET MAF supports declarative workflows ✅
- **Missing**: .NET declarative agent definitions from markdown ⭐ **← This is what we're building!**

### 🔗 Official Links
- **Declarative Workflows (.NET)**: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/declarative-workflows
- **Python Declarative Samples**: https://github.com/microsoft/agent-framework/tree/main/python/samples/getting_started/declarative
- **AgentSchema Spec**: https://microsoft.github.io/AgentSchema/
- **NuGet Package**: https://www.nuget.org/packages/Microsoft.Agents.AI.Workflows.Declarative

---

## Implementation Strategy

### Phase 1: Research & Design ✅
- [x] Identify markdown parsing libraries (Markdig + YamlDotNet recommended)
- [x] Study GitHub Copilot agent format
- [x] Review Microsoft Agent Framework configuration patterns
- [x] Analyze similar projects and standards
- [x] Test markdown parsing with Markdig + YamlDotNet (successful!)
- [x] Identify gap in .NET ecosystem for declarative agent definitions

### Phase 2: Core Development ✅
- [x] Create markdown schema for agent definitions
- [x] Implement markdown parser using Markdig
- [x] Build YAML frontmatter deserializer with YamlDotNet
- [x] Map markdown structure to Microsoft Agent Framework agent objects
- [x] Handle tool definitions, instructions, and metadata

### Phase 3: Factory Pattern Implementation ✅
- [x] Design factory interface for agent creation
- [x] Implement markdown-to-agent factory
- [x] Add validation and error handling
- [x] Support multiple agent types and configurations
- [x] **Implement Chain of Responsibility pattern for provider selection**

### Phase 4: Testing & Documentation ✅
- [x] Create example agent markdown files
- [x] Document usage and examples
- [ ] Write unit tests for parser and factory
- [ ] Build integration tests with Agent Framework

### Phase 5: Extension Features
- [ ] Support agent workflow definitions
- [ ] Enable multi-agent orchestration via markdown
- [ ] Add template system for common agent patterns
- [ ] Implement hot-reload for development

---

## Chain of Responsibility Pattern for Provider Selection

**New in Phase 3**: The factory now implements the **Chain of Responsibility** design pattern for intelligent provider routing and automatic fallback.

### Key Features

✅ **Automatic Fallback**: If a provider cannot handle a model, the next provider in the chain is tried automatically  
✅ **Model-Based Routing**: Each provider determines if it can handle a specific model (e.g., `gpt-4o`, `llama-3.2`)  
✅ **Configurable Chain Order**: Define provider priority in configuration  
✅ **Resilient to Failures**: Gracefully handles provider outages by falling back to alternatives  
✅ **Zero Code Changes**: Just configure the provider chain, the factory handles the rest  

### Configuration Example

```json
{
  "agentFactory": {
    "defaultProvider": "azureOpenAI",
    "providerChain": ["azureOpenAI", "openAI", "githubModels"],
    "enableLogging": true
  },
  "providers": {
    "azureOpenAI": {
      "endpoint": "https://my-resource.openai.azure.com",
      "deploymentName": "gpt-4"
    },
    "openAI": {
      "apiKey": "sk-...",
      "model": "gpt-4o-mini"
    },
    "githubModels": {
      "token": "ghp_...",
      "model": "llama-3.2"
    }
  }
}
```

### How It Works

1. **Request**: `CreateChatClient("llama-3.2")`
2. **Azure OpenAI**: ❌ Cannot handle this model
3. **OpenAI**: ❌ Model not in catalog
4. **GitHub Models**: ✅ Supports llama-3.2 → Returns client

### Documentation

- [CHAIN_OF_RESPONSIBILITY.md](./AgentFramework.Factory.TestConsole/CHAIN_OF_RESPONSIBILITY.md) - Complete pattern documentation
- [CHAIN_EXAMPLE.md](./AgentFramework.Factory.TestConsole/CHAIN_EXAMPLE.md) - Usage examples and test cases

---

## Technical Stack

- **Language:** C# / .NET
- **Markdown Parser:** Markdig
- **YAML Parser:** YamlDotNet
- **Agent Framework:** Microsoft Agent Framework
- **Target Platform:** .NET 8+

---

## Research Resources

### Context7 Documentation
- `/microsoft/agent-framework` (1,177 code snippets)
- `/websites/learn_microsoft_en-us_agent-framework` (2,282 snippets, score: 81.2)

### Key Learning Materials
- Microsoft Agent Framework official documentation
- GitHub Copilot agent format specification
- AGENTS.md and MAGI standards
- Markdig and YamlDotNet usage patterns
- Real-world agent configuration examples

---

## Next Steps

1. **Experiment with Markdig + YamlDotNet** - Create proof-of-concept parser
2. **Define agent markdown schema** - Establish structure and conventions
3. **Build minimal factory** - Convert simple markdown to Agent Framework objects
4. **Create sample agents** - Test with various agent types and configurations
5. **Iterate and refine** - Improve based on real-world usage patterns

---

## Questions to Explore

1. Should we support both GitHub Copilot format AND custom extensions?
2. How to handle tool definitions - inline markdown vs. external references?
3. What level of validation should occur at parse time vs. runtime?
4. Should we support agent templates and inheritance in markdown?
5. How to integrate with existing Agent Framework tooling and workflows?

---

## License & Contributing

_To be determined based on project scope and organizational requirements._

---

**Last Updated:** 2026-01-30  
**Project Status:** Phase 3 Complete - Core Implementation with Chain of Responsibility Pattern
