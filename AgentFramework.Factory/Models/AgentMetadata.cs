using YamlDotNet.Serialization;

namespace AgentFramework.Factory.Models;

/// <summary>
/// Metadata extracted from YAML frontmatter in markdown agent definitions
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
    public double? Temperature { get; set; }

    [YamlMember(Alias = "max_tokens")]
    public int? MaxTokens { get; set; }

    [YamlMember(Alias = "top_p")]
    public double? TopP { get; set; }

    [YamlMember(Alias = "frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    [YamlMember(Alias = "presence_penalty")]
    public double? PresencePenalty { get; set; }

    [YamlMember(Alias = "tools")]
    public List<string> Tools { get; set; } = new();

    [YamlMember(Alias = "provider")]
    public string? Provider { get; set; }
}
