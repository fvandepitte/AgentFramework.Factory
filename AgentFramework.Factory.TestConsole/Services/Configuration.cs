using YamlDotNet.Serialization;

namespace AgentFramework.Factory.TestConsole.Services;

/// <summary>
/// Root configuration model for the application
/// </summary>
public class AppConfiguration
{
    public AgentFactoryConfiguration AgentFactory { get; set; } = new();
    public ProvidersConfiguration Providers { get; set; } = new();
    public List<AgentConfigurationEntry> Agents { get; set; } = new();
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
    public bool EnableLogging { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
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

public class AzureOpenAIConfiguration
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4o-mini";
    public string ApiVersion { get; set; } = "2024-08-01-preview";
}

public class OpenAIConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}

public class GitHubModelsConfiguration
{
    public string Token { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
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
}
