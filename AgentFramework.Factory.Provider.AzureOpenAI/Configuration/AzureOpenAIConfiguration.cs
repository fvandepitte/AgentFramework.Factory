namespace AgentFramework.Factory.Provider.AzureOpenAI.Configuration;

/// <summary>
/// Configuration options for Azure OpenAI provider
/// </summary>
public class AzureOpenAIConfiguration
{
    /// <summary>
    /// Azure OpenAI service endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// API key for authentication (optional if using Azure credentials)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name for the model
    /// </summary>
    public string DeploymentName { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// API version to use
    /// </summary>
    public string ApiVersion { get; set; } = "2024-08-01-preview";
}
