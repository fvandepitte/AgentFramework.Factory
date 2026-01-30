using Microsoft.Extensions.AI;

namespace AgentFramework.Factory.TestConsole.Services.Tools;

/// <summary>
/// Interface for providing tools to agents
/// </summary>
public interface IToolProvider
{
    /// <summary>
    /// Gets the name of this tool provider
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the type of tools this provider handles (e.g., "local", "mcp")
    /// </summary>
    string Type { get; }
    
    /// <summary>
    /// Determines if this provider can handle the specified tool
    /// </summary>
    bool CanProvide(string toolName);
    
    /// <summary>
    /// Gets tools from this provider
    /// </summary>
    IEnumerable<AITool> GetTools(IEnumerable<string> toolNames);
    
    /// <summary>
    /// Gets all available tools from this provider
    /// </summary>
    IEnumerable<AITool> GetAllTools();
}
