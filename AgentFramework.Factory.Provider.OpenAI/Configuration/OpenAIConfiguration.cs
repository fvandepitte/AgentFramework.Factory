namespace AgentFramework.Factory.Provider.OpenAI.Configuration;

/// <summary>
/// Configuration options for OpenAI provider
/// </summary>
public class OpenAIConfiguration
{
    /// <summary>
    /// OpenAI API key for authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default model to use (can be overridden per agent)
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";
}
