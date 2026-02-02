namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Fluent API for programmatic agent creation
/// </summary>
public interface IAgentBuilder
{
    /// <summary>
    /// Sets the agent name
    /// </summary>
    IAgentBuilder WithName(string name);

    /// <summary>
    /// Sets the agent description
    /// </summary>
    IAgentBuilder WithDescription(string description);

    /// <summary>
    /// Sets the model to use
    /// </summary>
    IAgentBuilder WithModel(string model);

    /// <summary>
    /// Sets the agent instructions
    /// </summary>
    IAgentBuilder WithInstructions(string instructions);

    /// <summary>
    /// Adds tools to the agent
    /// </summary>
    IAgentBuilder WithTools(params string[] tools);

    /// <summary>
    /// Sets the temperature parameter
    /// </summary>
    IAgentBuilder WithTemperature(double temperature);

    /// <summary>
    /// Sets the max tokens parameter
    /// </summary>
    IAgentBuilder WithMaxTokens(int maxTokens);

    /// <summary>
    /// Sets the top-p parameter
    /// </summary>
    IAgentBuilder WithTopP(double topP);

    /// <summary>
    /// Sets the frequency penalty parameter
    /// </summary>
    IAgentBuilder WithFrequencyPenalty(double frequencyPenalty);

    /// <summary>
    /// Sets the presence penalty parameter
    /// </summary>
    IAgentBuilder WithPresencePenalty(double presencePenalty);

    /// <summary>
    /// Sets the provider override
    /// </summary>
    IAgentBuilder WithProvider(string provider);

    /// <summary>
    /// Builds the agent
    /// </summary>
    ILoadedAgent Build();
}
