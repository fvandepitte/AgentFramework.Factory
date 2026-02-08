using AgentFramework.Factory.Abstractions;

namespace AgentFramework.Factory.Models;

/// <summary>
/// Runtime agent representation after merging metadata and configuration overrides
/// </summary>
public class LoadedAgent : ILoadedAgent
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public IReadOnlyList<string> Tools { get; init; } = Array.Empty<string>();
    public double Temperature { get; init; } = 0.7;
    public int? MaxTokens { get; init; }
    public double? TopP { get; init; }
    public double? FrequencyPenalty { get; init; }
    public double? PresencePenalty { get; init; }
    public string? Provider { get; init; }
    public string? SourceFile { get; init; }
}
