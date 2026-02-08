using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Models;
using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.TestConsole.Services;

/// <summary>
/// Wrapper around IMarkdownAgentFactory that adds TestConsole-specific configuration loading methods
/// </summary>
public class MarkdownAgentFactory
{
    private readonly AppConfiguration configuration;
    private readonly IMarkdownAgentFactory coreFactory;

    public MarkdownAgentFactory(
        IOptions<AppConfiguration> configOptions,
        IMarkdownAgentFactory coreFactory)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(coreFactory);
        
        this.configuration = configOptions.Value;
        this.coreFactory = coreFactory;
    }

    /// <summary>
    /// Load all agents defined in the TestConsole configuration
    /// </summary>
    public List<LoadedAgent> LoadAgentsFromConfiguration()
    {
        var agents = new List<LoadedAgent>();

        foreach (var agentConfig in configuration.Agents.Where(a => a.Enabled))
        {
            try
            {
                var agent = LoadAgentFromMarkdown(agentConfig);
                agents.Add(agent);
                
                if (configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ✓ Loaded: {agent.Name} from {agentConfig.MarkdownPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Failed to load {agentConfig.Name}: {ex.Message}");
            }
        }

        return agents;
    }

    /// <summary>
    /// Load an agent from TestConsole configuration entry
    /// </summary>
    public LoadedAgent LoadAgentFromMarkdown(AgentConfigurationEntry config)
    {
        // Load the agent using core factory
        var loadedAgent = coreFactory.LoadAgentFromFile(config.MarkdownPath, config.Provider);
        
        // Apply configuration overrides and add SourceFile
        var result = new LoadedAgent
        {
            Name = config.Name ?? loadedAgent.Name,
            Description = loadedAgent.Description,
            Model = config.Overrides?.Model ?? loadedAgent.Model,
            Temperature = config.Overrides?.Temperature ?? loadedAgent.Temperature,
            MaxTokens = config.Overrides?.MaxTokens ?? loadedAgent.MaxTokens,
            TopP = config.Overrides?.TopP ?? loadedAgent.TopP,
            FrequencyPenalty = config.Overrides?.FrequencyPenalty ?? loadedAgent.FrequencyPenalty,
            PresencePenalty = config.Overrides?.PresencePenalty ?? loadedAgent.PresencePenalty,
            Instructions = loadedAgent.Instructions,
            Tools = loadedAgent.Tools,
            Provider = config.Provider ?? configuration.AgentFactory.DefaultProvider,
            SourceFile = config.MarkdownPath
        };

        return result;
    }

    /// <summary>
    /// Discover all markdown files in the configured directory
    /// </summary>
    public List<string> DiscoverAgentFiles()
    {
        var path = configuration.AgentFactory.AgentDefinitionsPath;
        var pattern = configuration.AgentFactory.AgentFilePattern;

        if (!Directory.Exists(path))
        {
            return new List<string>();
        }

        return Directory.GetFiles(path, pattern, SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    /// Delegate to core factory for standard loading
    /// </summary>
    public ILoadedAgent LoadAgentFromFile(string markdownPath, string? provider = null)
    {
        return coreFactory.LoadAgentFromFile(markdownPath, provider);
    }

    /// <summary>
    /// Delegate to core factory for parsing
    /// </summary>
    public ILoadedAgent ParseMarkdown(string markdownContent, string? provider = null)
    {
        return coreFactory.ParseMarkdown(markdownContent, provider);
    }
}
