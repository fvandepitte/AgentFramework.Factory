namespace AgentFramework.Factory.Configuration;

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
    
    /// <summary>
    /// Chain of providers to try in order (e.g., ["azureOpenAI", "openAI", "githubModels"])
    /// If not specified, only the DefaultProvider is used
    /// </summary>
    public List<string> ProviderChain { get; set; } = new();
    
    public bool EnableLogging { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public bool EnableToolDiscovery { get; set; } = true;
    public List<string> ToolAssemblies { get; set; } = new();
}
