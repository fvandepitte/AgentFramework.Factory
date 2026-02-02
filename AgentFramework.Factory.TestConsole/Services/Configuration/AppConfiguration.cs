using AgentFramework.Factory.Provider.AzureOpenAI.Configuration;
using AgentFramework.Factory.Provider.OpenAI.Configuration;
using AgentFramework.Factory.Provider.GitHubModels.Configuration;
using YamlDotNet.Serialization;

namespace AgentFramework.Factory.TestConsole.Services.Configuration;

/// <summary>
/// Root configuration model for the application
/// </summary>
public class AppConfiguration
{
    public AgentFactoryConfiguration AgentFactory { get; set; } = new();
    public ProvidersConfiguration Providers { get; set; } = new();
    public List<AgentConfigurationEntry> Agents { get; set; } = new();
    public ToolsConfiguration Tools { get; set; } = new();
    public List<McpServerConfiguration> McpServers { get; set; } = new();
}

/// <summary>
/// Configuration for tool management
/// </summary>
public class ToolsConfiguration
{
    public bool EnableMcp { get; set; } = false;
    public List<ToolDefinition> RegisteredTools { get; set; } = new();
}

/// <summary>
/// Configuration for the agent factory behavior
/// </summary>
public class AgentFactoryConfiguration
{
    public string AgentDefinitionsPath { get; set; } = "./agents";
    public string AgentFilePattern { get; set; } = "*.md";
    public string OutputPath { get; set; } = "./generated";
    public bool AutoReload { get; set; } = false;
    public string DefaultProvider { get; set; } = "azureOpenAI";
    /// <summary>
    /// Chain of providers to try in order (e.g., ["azureOpenAI", "openAI", "githubModels"])
    /// If not specified, only the DefaultProvider is used
    /// </summary>
    public List<string> ProviderChain { get; set; } = new();
    public bool EnableLogging { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public bool EnableToolDiscovery { get; set; } = true;
    public List<string> ToolAssemblies { get; set; } = new();
}

/// <summary>
/// Configuration for LLM providers
/// </summary>
public class ProvidersConfiguration
{
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();
    public OpenAIConfiguration OpenAI { get; set; } = new();
    public GitHubModelsConfiguration GitHubModels { get; set; } = new();
}

/// <summary>
/// Configuration entry for individual agents
/// </summary>
public class AgentConfigurationEntry
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string MarkdownPath { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public AgentOverrides? Overrides { get; set; }
}

/// <summary>
/// Override settings for agent configuration
/// </summary>
public class AgentOverrides
{
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public double? TopP { get; set; }
    public double? FrequencyPenalty { get; set; }
    public double? PresencePenalty { get; set; }
    public string? Model { get; set; }
}

/// <summary>
/// Agent metadata parsed from YAML frontmatter
/// </summary>
public class AgentMetadata
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;
    
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
    
    [YamlMember(Alias = "model")]
    public string Model { get; set; } = string.Empty;
    
    [YamlMember(Alias = "temperature")]
    public double Temperature { get; set; } = 0.7;
    
    [YamlMember(Alias = "max_tokens")]
    public int? MaxTokens { get; set; }
    
    [YamlMember(Alias = "top_p")]
    public double? TopP { get; set; }
    
    [YamlMember(Alias = "frequency_penalty")]
    public double? FrequencyPenalty { get; set; }
    
    [YamlMember(Alias = "presence_penalty")]
    public double? PresencePenalty { get; set; }
    
    [YamlMember(Alias = "tools")]
    public List<string>? Tools { get; set; }
}

/// <summary>
/// Tool definition for agents
/// </summary>
public class ToolDefinition
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;
    
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = "local"; // "local", "mcp"
    
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }
    
    [YamlMember(Alias = "assembly")]
    public string? Assembly { get; set; }
    
    [YamlMember(Alias = "class")]
    public string? ClassName { get; set; }
    
    [YamlMember(Alias = "method")]
    public string? Method { get; set; }
    
    [YamlMember(Alias = "mcp_server")]
    public string? McpServer { get; set; }
}

/// <summary>
/// MCP server configuration
/// </summary>
public class McpServerConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "stdio"; // "stdio", "http"
    public string? Command { get; set; }
    public List<string>? Args { get; set; }
    public string? Url { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? Environment { get; set; }
}
