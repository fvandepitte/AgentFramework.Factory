namespace AgentFramework.Factory.Provider.GitHubModels.Configuration;

/// <summary>
/// Configuration options for GitHub Models provider
/// </summary>
public class GitHubModelsConfiguration
{
    /// <summary>
    /// GitHub personal access token for authentication
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Default model to use (can be overridden per agent)
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";
}
