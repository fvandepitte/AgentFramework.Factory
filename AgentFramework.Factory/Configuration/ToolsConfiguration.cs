namespace AgentFramework.Factory.Configuration;

/// <summary>
/// Configuration for tool management
/// </summary>
public class ToolsConfiguration
{
    public bool EnableMcp { get; set; } = false;
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
