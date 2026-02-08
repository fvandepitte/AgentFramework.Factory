namespace AgentFramework.Factory.Configuration;

/// <summary>
/// Configuration for tool management
/// </summary>
public class ToolsConfiguration
{
    /// <summary>
    /// Enable MCP (Model Context Protocol) tool discovery
    /// </summary>
    public bool EnableMcp { get; set; } = false;

    /// <summary>
    /// Enable local tool discovery using LocalToolAttribute
    /// </summary>
    public bool EnableLocalTools { get; set; } = true;

    /// <summary>
    /// Assemblies to scan for local tools (if empty, scans entry assembly)
    /// </summary>
    public List<string> ToolAssemblies { get; set; } = new();

    /// <summary>
    /// MCP connections configuration (key is connection name)
    /// </summary>
    public Dictionary<string, McpConnectionConfiguration> McpConnections { get; set; } = new();

    /// <summary>
    /// Explicitly registered tools (legacy, prefer attribute-based discovery)
    /// </summary>
    public List<ToolDefinition> RegisteredTools { get; set; } = new();
}

/// <summary>
/// Definition of a registered tool
/// </summary>
public class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}
