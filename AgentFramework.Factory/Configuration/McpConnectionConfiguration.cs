namespace AgentFramework.Factory.Configuration;

/// <summary>
/// Configuration for MCP (Model Context Protocol) connection
/// </summary>
public class McpConnectionConfiguration
{
    /// <summary>
    /// Name of the MCP connection
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of MCP connection: "stdio" or "http"
    /// </summary>
    public string Type { get; set; } = "stdio";

    /// <summary>
    /// Command to execute for stdio connections
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// Arguments for the command (stdio connections)
    /// </summary>
    public List<string>? Args { get; set; }

    /// <summary>
    /// URL for HTTP connections
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// HTTP headers for HTTP connections
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Environment variables for stdio connections
    /// </summary>
    public Dictionary<string, string>? Environment { get; set; }
}
