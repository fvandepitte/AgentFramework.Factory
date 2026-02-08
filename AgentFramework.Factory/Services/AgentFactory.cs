using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Configuration;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.Services;

/// <summary>
/// Factory for creating fully configured AIAgent instances from markdown definitions
/// </summary>
public class AgentFactory
{
    private readonly AgentFactoryConfiguration configuration;
    private readonly IMarkdownAgentFactory markdownFactory;
    private readonly ProviderFactory providerFactory;
    private readonly ToolFactory toolFactory;
    private readonly ILogger<AgentFactory> logger;

    public AgentFactory(
        IOptions<AgentFactoryConfiguration> configOptions,
        IMarkdownAgentFactory markdownFactory,
        ProviderFactory providerFactory,
        ToolFactory toolFactory,
        ILogger<AgentFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(logger);
        
        this.configuration = configOptions.Value;
        this.markdownFactory = markdownFactory ?? throw new ArgumentNullException(nameof(markdownFactory));
        this.providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        this.toolFactory = toolFactory ?? throw new ArgumentNullException(nameof(toolFactory));
        this.logger = logger;
    }

    /// <summary>
    /// Create a fully configured AIAgent from a LoadedAgent
    /// </summary>
    public AIAgent CreateAgent(ILoadedAgent loadedAgent)
    {
        // Create the chat client for this agent's provider
        var chatClient = providerFactory.CreateChatClientForAgent(loadedAgent);

        // TODO: ChatOptions (temperature, max_tokens, etc.) should be configured on the chat client
        // using a builder pattern, or passed at runtime via ChatClientAgentRunOptions.
        // For now, these are loaded from the markdown but not yet applied.
        // See: https://learn.microsoft.com/en-us/dotnet/api/microsoft.agents.ai.chatclientagentrunoptions.chatoptions

        // Get tools for this agent
        var tools = new List<AITool>();
        if (loadedAgent.Tools.Any())
        {
            tools.AddRange(toolFactory.GetToolsForAgent(loadedAgent.Tools));
            
            if (configuration.EnableLogging)
            {
                logger.LogInformation("Agent '{Name}' configured with {Count} tool(s)", loadedAgent.Name, tools.Count);
            }
        }

        // Create the agent using AsAIAgent extension method with tools passed directly
        // Note: Tools can also be passed via ChatOptions at runtime, but passing them here
        // makes them part of the agent's default configuration
        var agent = chatClient.AsAIAgent(
            instructions: loadedAgent.Instructions,
            name: loadedAgent.Name,
            tools: tools.Any() ? tools : null
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
    /// Get the MarkdownAgentFactory for direct access
    /// </summary>
    public IMarkdownAgentFactory GetMarkdownFactory() => markdownFactory;

    /// <summary>
    /// Get the ProviderFactory for direct access
    /// </summary>
    public ProviderFactory GetProviderFactory() => providerFactory;

    /// <summary>
    /// Get the ToolFactory for direct access
    /// </summary>
    public ToolFactory GetToolFactory() => toolFactory;

    /// <summary>
    /// Validate that a markdown file can be loaded as an agent
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateAgentFile(string markdownPath)
    {
        try
        {
            if (!File.Exists(markdownPath))
            {
                return (false, $"Markdown file not found: {markdownPath}");
            }

            // Try to load the agent to validate
            var loadedAgent = markdownFactory.LoadAgentFromFile(markdownPath);
            
            if (string.IsNullOrWhiteSpace(loadedAgent.Name))
            {
                return (false, "Agent name is required");
            }

            // Provider validation is now handled by the chain of responsibility
            // We'll validate when attempting to create the chat client

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
