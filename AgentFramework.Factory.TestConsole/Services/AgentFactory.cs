using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using CoreAgentFactory = AgentFramework.Factory.Services.AgentFactory;

namespace AgentFramework.Factory.TestConsole.Services;

/// <summary>
/// Wrapper around core AgentFactory that adds TestConsole-specific methods for working with configuration
/// </summary>
public class AgentFactory
{
    private readonly AppConfiguration configuration;
    private readonly MarkdownAgentFactory markdownFactory;
    private readonly CoreAgentFactory coreFactory;

    public AgentFactory(
        IOptions<AppConfiguration> configOptions,
        MarkdownAgentFactory markdownFactory,
        CoreAgentFactory coreFactory)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(markdownFactory);
        ArgumentNullException.ThrowIfNull(coreFactory);
        
        this.configuration = configOptions.Value;
        this.markdownFactory = markdownFactory;
        this.coreFactory = coreFactory;
    }

    /// <summary>
    /// Load and create an agent by name from configuration
    /// </summary>
    public AIAgent CreateAgentByName(string name)
    {
        var agentConfig = configuration.Agents.FirstOrDefault(a => 
            a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && a.Enabled);

        if (agentConfig == null)
        {
            throw new InvalidOperationException(
                $"Agent '{name}' not found in configuration or is disabled");
        }

        var loadedAgent = markdownFactory.LoadAgentFromMarkdown(agentConfig);
        return coreFactory.CreateAgent(loadedAgent);
    }

    /// <summary>
    /// Load and create all enabled agents from configuration
    /// </summary>
    public Dictionary<string, AIAgent> CreateAllAgents()
    {
        var agents = new Dictionary<string, AIAgent>();
        var loadedAgents = markdownFactory.LoadAgentsFromConfiguration();

        foreach (var loadedAgent in loadedAgents)
        {
            try
            {
                var agent = coreFactory.CreateAgent(loadedAgent);
                agents[loadedAgent.Name] = agent;

                if (configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ✓ Created agent: {loadedAgent.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Failed to create agent {loadedAgent.Name}: {ex.Message}");
            }
        }

        return agents;
    }

    /// <summary>
    /// Validate that an agent can be created (checks configuration and provider)
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateAgent(string agentName)
    {
        try
        {
            var agentConfig = configuration.Agents.FirstOrDefault(a => 
                a.Name.Equals(agentName, StringComparison.OrdinalIgnoreCase));

            if (agentConfig == null)
            {
                return (false, $"Agent '{agentName}' not found in configuration");
            }

            if (!agentConfig.Enabled)
            {
                return (false, $"Agent '{agentName}' is disabled");
            }

            if (!File.Exists(agentConfig.MarkdownPath))
            {
                return (false, $"Markdown file not found: {agentConfig.MarkdownPath}");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Delegate to core factory for markdown-based creation
    /// </summary>
    public AIAgent CreateAgentFromMarkdown(string markdownPath, string? provider = null)
    {
        return coreFactory.CreateAgentFromMarkdown(markdownPath, provider);
    }

    /// <summary>
    /// Get the core AgentFactory for direct access
    /// </summary>
    public CoreAgentFactory GetCoreFactory() => coreFactory;
}
