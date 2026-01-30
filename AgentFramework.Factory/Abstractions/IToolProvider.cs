using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Defines a tool provider that can supply AI tools to agents
/// </summary>
public interface IToolProvider
{
    /// <summary>
    /// Gets the name of this tool provider
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of this tool provider (e.g., "local", "mcp")
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Determines if this provider can supply the specified tool
    /// </summary>
    /// <param name="toolName">The name of the tool</param>
    /// <returns>True if this provider can supply the tool</returns>
    bool CanProvide(string toolName);

    /// <summary>
    /// Gets the specified tools from this provider
    /// </summary>
    /// <param name="toolNames">Names of the tools to retrieve</param>
    /// <returns>Collection of AITool instances</returns>
    IEnumerable<AITool> GetTools(IEnumerable<string> toolNames);

    /// <summary>
    /// Gets all available tools from this provider
    /// </summary>
    /// <returns>Collection of all AITool instances</returns>
    IEnumerable<AITool> GetAllTools();
}
