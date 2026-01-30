namespace AgentFramework.Factory.TestConsole.Services.Models;

/// <summary>
/// Represents a loaded agent with its configuration
/// </summary>
public class LoadedAgent
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public double? TopP { get; set; }
    public double? FrequencyPenalty { get; set; }
    public double? PresencePenalty { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
