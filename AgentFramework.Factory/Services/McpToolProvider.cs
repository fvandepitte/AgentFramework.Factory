using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace AgentFramework.Factory.Services;

/// <summary>
/// Provides tools from MCP (Model Context Protocol) servers
/// </summary>
public class McpToolProvider : IToolProvider, IAsyncDisposable
{
    private readonly ToolsConfiguration configuration;
    private readonly ILogger<McpToolProvider> logger;
    private readonly Dictionary<string, AITool> mcpTools = new();
    private readonly Dictionary<string, string> toolToServerMap = new(); // Maps tool name -> server name
    private readonly Dictionary<string, McpClient> mcpClients = new();
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool initialized = false;

    public McpToolProvider(
        IOptions<ToolsConfiguration> configOptions,
        ILogger<McpToolProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(logger);
        
        this.configuration = configOptions.Value;
        this.logger = logger;
        
        if (configuration.EnableMcp && configuration.McpConnections.Any())
        {
            // Initialize synchronously for now - proper async initialization would require IHostedService
            InitializeMcpServers().GetAwaiter().GetResult();
        }
    }

    public string Name => "MCP";
    public string Type => "mcp";

    public bool CanProvide(string toolName)
    {
        return mcpTools.ContainsKey(toolName);
    }

    public IEnumerable<AITool> GetTools(IEnumerable<string> toolNames)
    {
        foreach (var toolName in toolNames)
        {
            if (mcpTools.TryGetValue(toolName, out var tool))
            {
                yield return tool;
            }
        }
    }

    public IEnumerable<AITool> GetAllTools()
    {
        return mcpTools.Values;
    }

    /// <summary>
    /// Gets the names of all connected MCP servers
    /// </summary>
    public IEnumerable<string> GetServerNames()
    {
        return mcpClients.Keys;
    }

    /// <summary>
    /// Gets all tools from a specific MCP server
    /// </summary>
    public IEnumerable<AITool> GetToolsFromServer(string serverName)
    {
        return mcpTools
            .Where(kvp => toolToServerMap.TryGetValue(kvp.Key, out var server) && 
                          server.Equals(serverName, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Value);
    }

    /// <summary>
    /// Checks if a server with the given name exists
    /// </summary>
    public bool HasServer(string serverName)
    {
        return mcpClients.Keys.Any(k => k.Equals(serverName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a tool by server-qualified name (e.g., "github/search_repositories")
    /// </summary>
    public AITool? GetToolByQualifiedName(string serverName, string toolName)
    {
        if (mcpTools.TryGetValue(toolName, out var tool) &&
            toolToServerMap.TryGetValue(toolName, out var server) &&
            server.Equals(serverName, StringComparison.OrdinalIgnoreCase))
        {
            return tool;
        }
        return null;
    }

    /// <summary>
    /// Gets the server name for a given tool name
    /// </summary>
    public string? GetServerNameForTool(string toolName)
    {
        return toolToServerMap.TryGetValue(toolName, out var server) ? server : null;
    }

    /// <summary>
    /// Initializes connections to configured MCP servers
    /// </summary>
    private async Task InitializeMcpServers()
    {
        await initializationLock.WaitAsync();
        try
        {
            if (initialized)
                return;

            logger.LogInformation("Initializing MCP connections...");

            foreach (var (serverName, serverConfig) in configuration.McpConnections)
            {
                try
                {
                    await ConnectToServerAsync(serverName, serverConfig);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to connect to MCP server '{ServerName}'", serverName);
                }
            }

            initialized = true;

            logger.LogInformation("MCP tool provider initialized with {ToolCount} tool(s) from {ServerCount} server(s)", 
                mcpTools.Count, mcpClients.Count);
        }
        finally
        {
            initializationLock.Release();
        }
    }

    /// <summary>
    /// Connects to an MCP server and discovers its tools
    /// </summary>
    private async Task ConnectToServerAsync(string serverName, McpConnectionConfiguration serverConfig)
    {
        IClientTransport transport;

        // Create transport based on server type
        if (serverConfig.Type.Equals("http", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(serverConfig.Url))
            {
                throw new InvalidOperationException($"HTTP MCP server '{serverName}' must have a URL configured");
            }

            // Create HttpClient with custom headers if configured
            var httpClient = new HttpClient();
            if (serverConfig.Headers != null)
            {
                foreach (var header in serverConfig.Headers)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            transport = new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(serverConfig.Url),
                    TransportMode = HttpTransportMode.AutoDetect,
                    ConnectionTimeout = TimeSpan.FromSeconds(30)
                },
                httpClient,
                ownsHttpClient: true
            );
        }
        else if (serverConfig.Type.Equals("stdio", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(serverConfig.Command))
            {
                throw new InvalidOperationException($"Stdio MCP server '{serverName}' must have a command configured");
            }

            var envVars = serverConfig.Environment?.ToDictionary(kv => kv.Key, kv => (string?)kv.Value) 
                ?? new Dictionary<string, string?>();
            
            transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = serverName,
                Command = serverConfig.Command,
                Arguments = serverConfig.Args ?? [],
                EnvironmentVariables = envVars
            });
        }
        else
        {
            throw new NotSupportedException($"MCP server type '{serverConfig.Type}' is not supported. Use 'http' or 'stdio'.");
        }

        // Create and connect client
        var client = await McpClient.CreateAsync(
            transport,
            new McpClientOptions
            {
                ClientInfo = new Implementation
                {
                    Name = "AgentFramework.Factory",
                    Version = "1.0.0"
                }
            }
        );

        mcpClients[serverName] = client;

        logger.LogInformation("Connected to MCP server: {ServerName} ({ServerInfo} v{Version})", 
            serverName, client.ServerInfo.Name, client.ServerInfo.Version);

        // Discover tools
        await DiscoverToolsFromServerAsync(serverName, client);
    }

    /// <summary>
    /// Discovers and registers tools from an MCP server
    /// </summary>
    private async Task DiscoverToolsFromServerAsync(string serverName, McpClient client)
    {
        var tools = await client.ListToolsAsync();

        foreach (var mcpTool in tools)
        {
            // McpClientTool already implements AITool - no conversion needed!
            var toolKey = mcpTool.Name;
            
            if (mcpTools.ContainsKey(toolKey))
            {
                logger.LogWarning("Tool '{ToolName}' already registered, skipping duplicate from server '{ServerName}'", 
                    toolKey, serverName);
                continue;
            }

            mcpTools[toolKey] = mcpTool;
            toolToServerMap[toolKey] = serverName;

            logger.LogInformation("Registered MCP tool: {ToolName} from {ServerName}", toolKey, serverName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in mcpClients.Values)
        {
            try
            {
                await client.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
        
        mcpClients.Clear();
        mcpTools.Clear();
        toolToServerMap.Clear();
        initializationLock.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
