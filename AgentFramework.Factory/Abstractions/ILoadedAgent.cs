namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Represents an agent loaded from markdown with all its configuration
/// </summary>
public interface ILoadedAgent
{
    /// <summary>
    /// The name of the agent
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The agent's description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The model to use for this agent
    /// </summary>
    string Model { get; }

    /// <summary>
    /// The agent's instructions (persona)
    /// </summary>
    string Instructions { get; }

    /// <summary>
    /// Names of tools this agent can use
    /// </summary>
    IReadOnlyList<string> Tools { get; }

    /// <summary>
    /// Temperature setting for LLM generation
    /// </summary>
    double Temperature { get; }

    /// <summary>
    /// Maximum number of tokens for generation
    /// </summary>
    int? MaxTokens { get; }

    /// <summary>
    /// Top-p sampling parameter
    /// </summary>
    double? TopP { get; }

    /// <summary>
    /// Frequency penalty parameter
    /// </summary>
    double? FrequencyPenalty { get; }

    /// <summary>
    /// Presence penalty parameter
    /// </summary>
    double? PresencePenalty { get; }

    /// <summary>
    /// Optional provider override for this agent
    /// </summary>
    string? Provider { get; }
}
