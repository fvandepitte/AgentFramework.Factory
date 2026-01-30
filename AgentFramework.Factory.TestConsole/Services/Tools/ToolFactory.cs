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
        
        // Get MCP provider for server-specific lookups
        var mcpProvider = toolProviders.OfType<McpToolProvider>().FirstOrDefault();

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
                            if (configuration.AgentFactory.EnableLogging)
                            {
                                Console.WriteLine($"  ✓ Added tool '{name}' from {prov.Name} provider (wildcard *)");
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
                    if (mcpProvider != null)
                    {
                        foreach (var tool in mcpProvider.GetAllTools())
                        {
                            var name = GetToolName(tool);
                            if (addedToolNames.Add(name))
                            {
                                tools.Add(tool);
                                if (configuration.AgentFactory.EnableLogging)
                                {
                                    Console.WriteLine($"  ✓ Added tool '{name}' from MCP provider (wildcard mcp/*)");
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
                                if (configuration.AgentFactory.EnableLogging)
                                {
                                    Console.WriteLine($"  ✓ Added tool '{name}' from Local provider (wildcard local/*)");
                                }
                            }
                        }
                    }
                    continue;
                }

                // Otherwise, treat as MCP server name pattern (e.g., "github/*")
                if (mcpProvider != null && mcpProvider.HasServer(pattern))
                {
                    foreach (var tool in mcpProvider.GetToolsFromServer(pattern))
                    {
                        var name = GetToolName(tool);
                        if (addedToolNames.Add(name))
                        {
                            tools.Add(tool);
                            if (configuration.AgentFactory.EnableLogging)
                            {
                                Console.WriteLine($"  ✓ Added tool '{name}' from MCP server '{pattern}' (wildcard {toolName})");
                            }
                        }
                    }
                }
                else if (configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ⚠ No MCP server found matching pattern '{pattern}'");
                }
                continue;
            }

            // Handle server-qualified tool names (e.g., "github/search_repositories")
            if (toolName.Contains('/'))
            {
                var parts = toolName.Split('/', 2);
                var serverName = parts[0];
                var actualToolName = parts[1];

                if (mcpProvider != null)
                {
                    var tool = mcpProvider.GetToolByQualifiedName(serverName, actualToolName);
                    if (tool != null)
                    {
                        var name = GetToolName(tool);
                        if (addedToolNames.Add(name))
                        {
                            tools.Add(tool);
                            if (configuration.AgentFactory.EnableLogging)
                            {
                                Console.WriteLine($"  ✓ Added tool '{name}' from MCP server '{serverName}'");
                            }
                        }
                        continue;
                    }
                }
                notFound.Add(toolName);
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
                        
                        if (configuration.AgentFactory.EnableLogging)
                        {
                            Console.WriteLine($"  ✓ Resolved tool '{toolName}' from {provider.Name} provider");
                        }
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
    /// Extracts the name from an AITool (handles both AIFunction and McpClientTool)
    /// </summary>
    private static string GetToolName(AITool tool)
    {
        if (tool is AIFunction aiFunction)
        {
            return aiFunction.Name;
        }
        
        // For McpClientTool and other types, try to get name via reflection or type name
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
