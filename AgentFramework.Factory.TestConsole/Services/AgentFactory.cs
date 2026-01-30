using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace AgentFramework.Factory.TestConsole.Services;

/// <summary>
/// Factory for creating fully configured AIAgent instances from markdown definitions
/// </summary>
public class AgentFactory
{
    private readonly AppConfiguration configuration;
    private readonly MarkdownAgentFactory markdownFactory;
    private readonly ProviderFactory providerFactory;

    public AgentFactory(AppConfiguration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.markdownFactory = new MarkdownAgentFactory(configuration);
        this.providerFactory = new ProviderFactory(configuration);
    }

    /// <summary>
    /// Create a fully configured AIAgent from a LoadedAgent
    /// </summary>
    public AIAgent CreateAgent(LoadedAgent loadedAgent)
    {
        // Create the chat client for this agent's provider
        var chatClient = providerFactory.CreateChatClientForAgent(loadedAgent);

        // Build chat options from agent configuration
        var chatOptions = new ChatOptions();

        if (loadedAgent.Temperature > 0)
        {
            chatOptions.Temperature = (float)loadedAgent.Temperature;
        }

        if (loadedAgent.MaxTokens.HasValue)
        {
            chatOptions.MaxOutputTokens = loadedAgent.MaxTokens.Value;
        }

        if (loadedAgent.TopP.HasValue)
        {
            chatOptions.TopP = (float)loadedAgent.TopP.Value;
        }

        if (loadedAgent.FrequencyPenalty.HasValue)
        {
            chatOptions.FrequencyPenalty = (float)loadedAgent.FrequencyPenalty.Value;
        }

        if (loadedAgent.PresencePenalty.HasValue)
        {
            chatOptions.PresencePenalty = (float)loadedAgent.PresencePenalty.Value;
        }

        // Create the agent using AsAIAgent extension method which properly sets name and options
        var agent = chatClient.AsAIAgent(
            instructions: loadedAgent.Instructions,
            name: loadedAgent.Name
        );

        return agent;
    }

    /// <summary>
    /// Load and create an agent from a markdown file
    /// </summary>
    public AIAgent CreateAgentFromMarkdown(string markdownPath, string? provider = null)
    {
        var loadedAgent = markdownFactory.LoadAgentFromFile(markdownPath, provider);
        return CreateAgent(loadedAgent);
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
        return CreateAgent(loadedAgent);
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
                var agent = CreateAgent(loadedAgent);
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
    /// Get the MarkdownAgentFactory for direct access
    /// </summary>
    public MarkdownAgentFactory GetMarkdownFactory() => markdownFactory;

    /// <summary>
    /// Get the ProviderFactory for direct access
    /// </summary>
    public ProviderFactory GetProviderFactory() => providerFactory;

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

            var provider = agentConfig.Provider ?? configuration.AgentFactory.DefaultProvider;
            var (isProviderValid, providerError) = providerFactory.ValidateProvider(provider);

            if (!isProviderValid)
            {
                return (false, $"Provider validation failed: {providerError}");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
