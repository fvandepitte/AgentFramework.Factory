using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.Services;

/// <summary>
/// Factory for managing and retrieving tools from multiple providers
/// </summary>
public class ToolFactory
{
    private readonly AgentFactoryConfiguration configuration;
    private readonly List<IToolProvider> toolProviders;
    private readonly ILogger<ToolFactory> logger;

    public ToolFactory(
        IOptions<AgentFactoryConfiguration> configOptions,
        IEnumerable<IToolProvider> providers,
        ILogger<ToolFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(logger);
        
        this.configuration = configOptions.Value;
        this.toolProviders = providers.ToList();
        this.logger = logger;

        if (configuration.EnableLogging)
        {
            logger.LogInformation("ToolFactory initialized with {Count} provider(s)", toolProviders.Count);
            foreach (var provider in toolProviders)
            {
                logger.LogInformation("  - {Name} ({Type})", provider.Name, provider.Type);
            }
        }
    }

    /// <summary>
    /// Gets tools for a loaded agent based on its tool names.
    /// Supports wildcard patterns:
    /// - "*" or "all" - includes all tools from all providers
    /// - "mcp/*" - includes all tools from all MCP servers
    /// - "local/*" - includes all tools from the local provider
    /// - "github/*" - includes all tools from the MCP server named "github"
    /// - "github/search_repositories" - specific tool from the "github" MCP server
    /// </summary>
    public IEnumerable<AITool> GetToolsForAgent(IEnumerable<string> toolNames)
    {
        var tools = new List<AITool>();
        var notFound = new List<string>();
        var addedToolNames = new HashSet<string>();

        foreach (var toolName in toolNames)
        {
            // Handle wildcard patterns
            if (toolName == "*" || toolName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                // Include all tools from all providers
                foreach (var prov in toolProviders)
                {
                    foreach (var tool in prov.GetAllTools())
                    {
                        var name = GetToolName(tool);
                        if (addedToolNames.Add(name))
                        {
                            tools.Add(tool);
                            if (configuration.EnableLogging)
                            {
                                logger.LogInformation("  ✓ Added tool '{Name}' from {Provider} provider (wildcard *)", name, prov.Name);
                            }
                        }
                    }
                }
                continue;
            }

            // Handle provider/server wildcards (e.g., "mcp/*", "local/*", "github/*")
            if (toolName.EndsWith("/*"))
            {
                var pattern = toolName[..^2];
                
                // Check if it's a provider type pattern (mcp/*, local/*)
                if (pattern.Equals("mcp", StringComparison.OrdinalIgnoreCase))
                {
                    // All MCP tools from all servers
                    var mcpProviders = toolProviders.Where(p => p.Type.Equals("mcp", StringComparison.OrdinalIgnoreCase));
                    foreach (var prov in mcpProviders)
                    {
                        foreach (var tool in prov.GetAllTools())
                        {
                            var name = GetToolName(tool);
                            if (addedToolNames.Add(name))
                            {
                                tools.Add(tool);
                                if (configuration.EnableLogging)
                                {
                                    logger.LogInformation("  ✓ Added tool '{Name}' from MCP provider (wildcard mcp/*)", name);
                                }
                            }
                        }
                    }
                    continue;
                }
                
                if (pattern.Equals("local", StringComparison.OrdinalIgnoreCase))
                {
                    var localProvider = toolProviders.FirstOrDefault(p => p.Type.Equals("local", StringComparison.OrdinalIgnoreCase));
                    if (localProvider != null)
                    {
                        foreach (var tool in localProvider.GetAllTools())
                        {
                            var name = GetToolName(tool);
                            if (addedToolNames.Add(name))
                            {
                                tools.Add(tool);
                                if (configuration.EnableLogging)
                                {
                                    logger.LogInformation("  ✓ Added tool '{Name}' from Local provider (wildcard local/*)", name);
                                }
                            }
                        }
                    }
                    continue;
                }

                // Otherwise, treat as server-specific pattern
                // For MCP servers, we need provider-specific logic
                var mcpProvider = toolProviders.FirstOrDefault(p => p.Type.Equals("mcp", StringComparison.OrdinalIgnoreCase));
                if (mcpProvider != null)
                {
                    // Try to get tools matching the pattern - this requires provider-specific implementation
                    var serverTools = mcpProvider.GetTools(new[] { toolName });
                    foreach (var tool in serverTools)
                    {
                        var name = GetToolName(tool);
                        if (addedToolNames.Add(name))
                        {
                            tools.Add(tool);
                            if (configuration.EnableLogging)
                            {
                                logger.LogInformation("  ✓ Added tool '{Name}' (wildcard {Pattern})", name, toolName);
                            }
                        }
                    }
                }
                continue;
            }

            // Handle server-qualified tool names (e.g., "github/search_repositories")
            if (toolName.Contains('/'))
            {
                // Try each provider to see if it can handle qualified names
                var found = false;
                foreach (var prov in toolProviders)
                {
                    if (prov.CanProvide(toolName))
                    {
                        var tool = prov.GetTools(new[] { toolName }).FirstOrDefault();
                        if (tool != null)
                        {
                            var name = GetToolName(tool);
                            if (addedToolNames.Add(name))
                            {
                                tools.Add(tool);
                                if (configuration.EnableLogging)
                                {
                                    logger.LogInformation("  ✓ Added tool '{Name}' from {Provider}", name, prov.Name);
                                }
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                {
                    notFound.Add(toolName);
                }
                continue;
            }

            // Standard tool lookup (unqualified name)
            var provider = toolProviders.FirstOrDefault(p => p.CanProvide(toolName));
            if (provider != null)
            {
                var tool = provider.GetTools(new[] { toolName }).FirstOrDefault();
                if (tool != null)
                {
                    var name = GetToolName(tool);
                    if (addedToolNames.Add(name))
                    {
                        tools.Add(tool);
                        
                        if (configuration.EnableLogging)
                        {
                            logger.LogInformation("  ✓ Resolved tool '{ToolName}' from {Provider} provider", toolName, provider.Name);
                        }
                    }
                }
            }
            else
            {
                notFound.Add(toolName);
            }
        }

        if (notFound.Any() && configuration.EnableLogging)
        {
            logger.LogWarning("  ⚠ Could not find tools: {Tools}", string.Join(", ", notFound));
        }

        return tools;
    }

    /// <summary>
    /// Extracts the name from an AITool (handles both AIFunction and other types)
    /// </summary>
    private static string GetToolName(AITool tool)
    {
        if (tool is AIFunction aiFunction)
        {
            return aiFunction.Name;
        }
        
        // For other types, try to get name via reflection
        var nameProperty = tool.GetType().GetProperty("Name");
        if (nameProperty != null)
        {
            return nameProperty.GetValue(tool)?.ToString() ?? tool.GetType().Name;
        }
        
        return tool.GetType().Name;
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
