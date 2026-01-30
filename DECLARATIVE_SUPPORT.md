# Microsoft Agent Framework - Declarative Support Analysis

## Summary

The Microsoft Agent Framework has **different levels of declarative support** depending on the language and what you're trying to declare.

---

## Python: Full Declarative Support ✅

### Package
- **PyPI**: `agent-framework-declarative` (prerelease)
- **Installation**: `pip install agent-framework-declarative --pre`

### Capabilities
- Define **individual agents** in YAML
- Define **workflows** in YAML
- PowerFx expressions for dynamic bindings (`=Env.VARIABLE`)

### Example Agent Definition (Python)
```yaml
kind: Prompt
name: DiagnosticAgent
displayName: Diagnostic Agent
description: Example agent for diagnostics
instructions: "You are an assistant that provides diagnostics."
model:
  id: =Env.AZURE_OPENAI_MODEL
  connection:
    kind: remote
    endpoint: =Env.AZURE_AI_PROJECT_ENDPOINT
tools:
  - name: get_weather
    function: get_weather_function
```

### Usage in Python
```python
from agent_framework.declarative import AgentFactory

agent = AgentFactory().create_agent_from_yaml_path("agent.yaml")
response = agent.run("What can you do for me?")
```

### Resources
- [Python Declarative Samples](https://github.com/microsoft/agent-framework/tree/main/python/samples/getting_started/declarative)
- [DeepWiki: Declarative Agents (Python)](https://deepwiki.com/microsoft/agent-framework/4.9-declarative-agents-(python))

---

## .NET: Workflow-Focused Declarative Support ⚠️

### Package
- **NuGet**: `Microsoft.Agents.AI.Workflows.Declarative` (prerelease)
- **Installation**: `dotnet add package Microsoft.Agents.AI.Workflows.Declarative --prerelease`

### Capabilities
- Define **workflows** in YAML (sequential, concurrent, conditional)
- PowerFx expressions for workflow logic
- **Agents must be created in code first**, then orchestrated via declarative workflows

### Example Workflow Definition (.NET)
```yaml
version: 1.0
agents:
  - id: frenchTranslator
    type: chat
    model: azure-openai
    instructions: "Translate input text to French."
  - id: spanishTranslator
    type: chat
    model: azure-openai
    instructions: "Translate input text from French to Spanish."

workflow:
  steps:
    - agent: frenchTranslator
      input: "{{ input }}"
      output: "frenchText"
    - agent: spanishTranslator
      input: "{{ steps[0].result }}"
      output: "spanishText"
  outputs:
    final: "{{ steps[1].result }}"
```

### Usage in .NET (Hypothetical - needs verification)
```csharp
using Microsoft.Agents.AI.Workflows.Declarative;

var yaml = File.ReadAllText("translator-workflow.yaml");
var workflowDefinition = DeclarativeWorkflowLoader.LoadFromYaml(yaml);

var workflowRunner = new WorkflowRunner(workflowDefinition, /* services/providers */);
var result = await workflowRunner.RunAsync(new { input = "Hello, world!" });
```

### ⚠️ Limited Individual Agent Support
Unlike Python, .NET doesn't have a clear `AgentFactory.CreateFromYaml()` equivalent. The declarative support is primarily for **workflow orchestration**, not individual agent definition.

### Resources
- [Declarative Workflows Overview - Microsoft Learn](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/declarative-workflows)
- [DeepWiki: Declarative Workflows (.NET)](https://deepwiki.com/microsoft/agent-framework/3.4.5-declarative-workflows-(.net))
- [Agents in Workflows Tutorial](https://learn.microsoft.com/en-us/agent-framework/tutorials/workflows/agents-in-workflows)
- [GitHub Discussion on YAML Support](https://github.com/microsoft/agent-framework/discussions/1516)

---

## Comparison Table

| Feature | Python | .NET |
|---------|--------|------|
| **Declarative Agent Definitions** | ✅ Full Support | ❌ Code-first only |
| **Declarative Workflows** | ✅ Full Support | ✅ Full Support |
| **YAML Package** | `agent-framework-declarative` | `Microsoft.Agents.AI.Workflows.Declarative` |
| **PowerFx Expressions** | ✅ Yes | ✅ Yes |
| **Agent Factory from YAML** | ✅ `AgentFactory().create_agent_from_yaml_path()` | ❌ Not available |
| **Workflow Orchestration** | ✅ Yes | ✅ Yes |

---

## The Gap We're Filling

Our **Agent Framework Markdown Factory** project addresses the missing piece in the .NET ecosystem:

1. **GitHub Copilot** uses markdown files for agent definitions ✅
2. **Python MAF** supports declarative YAML agent definitions ✅
3. **.NET MAF** supports declarative workflow orchestration ✅
4. **Missing**: A clean way to define individual .NET agents from markdown/YAML files ❌

### Our Solution
Create a factory library that:
- Parses markdown files with YAML frontmatter (using Markdig + YamlDotNet)
- Maps markdown structure to Microsoft Agent Framework agent objects
- Provides a `.CreateAgentFromMarkdown()` API similar to Python's declarative approach
- Enables GitHub Copilot-style agent definitions for .NET developers

---

## Working Example (.NET)

We've successfully tested the parsing approach:

```csharp
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;

// Parse markdown with YAML frontmatter
var pipeline = new MarkdownPipelineBuilder()
    .UseYamlFrontMatter()
    .Build();

var document = Markdown.Parse(markdownContent, pipeline);
var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

// Extract and deserialize YAML
var yamlText = string.Join("\n", yamlBlock.Lines.Lines.Select(l => l.ToString()));
var deserializer = new DeserializerBuilder().Build();
var agentMetadata = deserializer.Deserialize<AgentMetadata>(yamlText);

// Extract markdown body as instructions
var markdownBody = Markdown.ToPlainText(document, pipeline);
```

**Result**: ✅ Successfully parses agent metadata and instructions from markdown!

---

## Next Steps

1. ✅ Verify markdown parsing works (Markdig + YamlDotNet)
2. ⏳ Design agent factory interface
3. ⏳ Map markdown to Agent Framework agent configuration
4. ⏳ Create sample agents and test with real MAF agents
5. ⏳ Build NuGet package for easy consumption

---

## References

- [Microsoft Agent Framework GitHub](https://github.com/microsoft/agent-framework)
- [Agent Framework Samples](https://github.com/microsoft/Agent-Framework-Samples)
- [AgentSchema Specification](https://microsoft.github.io/AgentSchema/)
- [Markdig Documentation](https://github.com/xoofx/markdig)
- [YamlDotNet Documentation](https://github.com/aaubry/YamlDotNet)

---

**Last Updated**: 2026-01-30  
**Status**: Declarative support confirmed for workflows; individual agent definitions need custom implementation for .NET
