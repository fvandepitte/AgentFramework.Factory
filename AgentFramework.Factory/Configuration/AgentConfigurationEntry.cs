namespace AgentFramework.Factory.Configuration;

/// <summary>
/// Configuration entry for individual agents
/// </summary>
public class AgentConfigurationEntry
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string MarkdownPath { get; set; } = string.Empty;
    public AgentOverrides Overrides { get; set; } = new();
}

/// <summary>
/// Override settings for agent configuration
/// </summary>
public class AgentOverrides
{
    public string? Model { get; set; }
    public string? Provider { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public double? TopP { get; set; }
    public double? FrequencyPenalty { get; set; }
    public double? PresencePenalty { get; set; }
    public List<string>? Tools { get; set; }
}
