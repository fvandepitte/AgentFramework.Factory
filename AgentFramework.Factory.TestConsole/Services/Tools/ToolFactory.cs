using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.TestConsole.Services.Tools;

/// <summary>
/// Factory for managing and retrieving tools from multiple providers
/// </summary>
public class ToolFactory
{
    private readonly AppConfiguration configuration;
    private readonly List<IToolProvider> toolProviders;

    public ToolFactory(
        IOptions<AppConfiguration> configOptions,
        IEnumerable<IToolProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(providers);
        
        this.configuration = configOptions.Value;
        this.toolProviders = providers.ToList();

        if (configuration.AgentFactory.EnableLogging)
        {
            Console.WriteLine($"  ℹ ToolFactory initialized with {toolProviders.Count} provider(s):");
            foreach (var provider in toolProviders)
            {
                Console.WriteLine($"    - {provider.Name} ({provider.Type})");
            }
        }
    }

    /// <summary>
    /// Gets tools for a loaded agent based on its tool names
    /// </summary>
    public IEnumerable<AITool> GetToolsForAgent(IEnumerable<string> toolNames)
    {
        var tools = new List<AITool>();
        var notFound = new List<string>();

        foreach (var toolName in toolNames)
        {
            var provider = toolProviders.FirstOrDefault(p => p.CanProvide(toolName));
            if (provider != null)
            {
                var tool = provider.GetTools(new[] { toolName }).FirstOrDefault();
                if (tool != null)
                {
                    tools.Add(tool);
                    
                    if (configuration.AgentFactory.EnableLogging)
                    {
                        Console.WriteLine($"  ✓ Resolved tool '{toolName}' from {provider.Name} provider");
                    }
                }
            }
            else
            {
                notFound.Add(toolName);
            }
        }

        if (notFound.Any() && configuration.AgentFactory.EnableLogging)
        {
            Console.WriteLine($"  ⚠ Could not find tools: {string.Join(", ", notFound)}");
        }

        return tools;
    }

    /// <summary>
    /// Gets all available tools from all providers
    /// </summary>
    public IEnumerable<AITool> GetAllAvailableTools()
    {
        return toolProviders.SelectMany(p => p.GetAllTools());
    }

    /// <summary>
    /// Gets all registered tool providers
    /// </summary>
    public IEnumerable<IToolProvider> GetProviders()
    {
        return toolProviders;
    }

    /// <summary>
    /// Gets a specific provider by type
    /// </summary>
    public IToolProvider? GetProvider(string type)
    {
        return toolProviders.FirstOrDefault(p => 
            p.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }
}
