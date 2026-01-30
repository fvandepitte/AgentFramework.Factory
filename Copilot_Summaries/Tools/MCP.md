# Model Context Protocol (MCP) Integration Guide

## Overview

The Agent Framework Factory is designed to support **Model Context Protocol (MCP)** integration, enabling agents to access external tools, resources, and data through standardized MCP servers.

## Current Status

🚧 **MCP integration is currently in placeholder status**. The infrastructure is in place, but the actual MCP SDK integration is pending.

### What's Implemented

- ✅ `McpToolProvider` placeholder class
- ✅ Configuration models for MCP servers
- ✅ Tool factory architecture supports multiple providers
- ✅ Agent factory prepared to use MCP tools

### What's Pending

- ⏳ ModelContextProtocol NuGet package integration
- ⏳ MCP client initialization and connection management
- ⏳ Tool discovery from MCP servers
- ⏳ AITool wrapper creation for MCP tools
- ⏳ Error handling and reconnection logic

## Planned Configuration

### appsettings.json

```json
{
  "tools": {
    "enableMcp": true,
    "registeredTools": []
  },
  "mcpServers": [
    {
      "name": "filesystem",
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "/workspace"],
      "environment": {
        "NODE_ENV": "production"
      }
    },
    {
      "name": "weather-api",
      "type": "http",
      "url": "http://localhost:3000/mcp",
      "environment": {}
    }
  ]
}
```

### Markdown Agent Configuration

```yaml
---
name: DataAnalyst
description: Agent with access to filesystem and APIs
model: gpt-4o-mini
temperature: 0.7
tools:
  - GetCurrentWeather      # Local tool
  - ReadFile               # MCP tool from filesystem server
  - WriteFile              # MCP tool from filesystem server
  - QueryDatabase          # MCP tool from custom server
---
```

## Implementation Plan

### Phase 1: Add MCP SDK

```bash
dotnet add package ModelContextProtocol
dotnet add package ModelContextProtocol.AspNetCore
```

### Phase 2: Initialize MCP Connections

In `McpToolProvider.cs`:

```csharp
private void InitializeMcpServers()
{
    foreach (var serverConfig in configuration.McpServers)
    {
        try
        {
            IMcpClient client;
            
            if (serverConfig.Type == "stdio")
            {
                // Connect via stdio
                client = new StdioMcpClient(
                    serverConfig.Command, 
                    serverConfig.Args?.ToArray() ?? Array.Empty<string>()
                );
            }
            else if (serverConfig.Type == "http")
            {
                // Connect via HTTP
                client = new HttpMcpClient(serverConfig.Url);
            }
            
            await client.ConnectAsync();
            
            // Store client
            mcpClients[serverConfig.Name] = client;
            
            // Discover tools
            await DiscoverToolsFromServer(serverConfig.Name, client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Failed to connect to MCP server {serverConfig.Name}: {ex.Message}");
        }
    }
}
```

### Phase 3: Discover Tools

```csharp
private async Task DiscoverToolsFromServer(string serverName, IMcpClient client)
{
    var tools = await client.ListToolsAsync();
    
    foreach (var mcpTool in tools)
    {
        // Create AITool wrapper
        var aiTool = CreateAIToolFromMcp(mcpTool, client);
        mcpTools[mcpTool.Name] = aiTool;
        
        if (configuration.AgentFactory.EnableLogging)
        {
            Console.WriteLine($"  ✓ Discovered MCP tool: {mcpTool.Name} from {serverName}");
        }
    }
}
```

### Phase 4: Tool Invocation

```csharp
private AITool CreateAIToolFromMcp(McpTool mcpTool, IMcpClient client)
{
    return AIFunctionFactory.Create(
        name: mcpTool.Name,
        description: mcpTool.Description,
        parameters: ConvertMcpParameters(mcpTool.InputSchema),
        implementation: async (args) =>
        {
            var result = await client.CallToolAsync(mcpTool.Name, args);
            return result.Content;
        }
    );
}
```

## MCP Server Examples

### Official MCP Servers

1. **Filesystem Server**
   ```bash
   npx -y @modelcontextprotocol/server-filesystem /workspace
   ```
   - Tools: read_file, write_file, list_directory, create_directory, delete_file

2. **GitHub Server**
   ```bash
   npx -y @modelcontextprotocol/server-github --token $GITHUB_TOKEN
   ```
   - Tools: create_issue, list_issues, create_pr, search_repositories

3. **PostgreSQL Server**
   ```bash
   npx -y @modelcontextprotocol/server-postgres postgres://user:pass@localhost/db
   ```
   - Tools: query, schema, list_tables

### Custom MCP Server in .NET

```csharp
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(null);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
```

## Security Considerations

### MCP Server Permissions

1. **Filesystem Access**: Limit MCP filesystem servers to specific directories
2. **Network Access**: Use allowlists for HTTP-based MCP servers
3. **Authentication**: Require tokens for sensitive MCP servers
4. **Rate Limiting**: Implement request limits per server

### Example Restricted Configuration

```json
{
  "mcpServers": [
    {
      "name": "restricted-filesystem",
      "type": "stdio",
      "command": "npx",
      "args": [
        "-y", 
        "@modelcontextprotocol/server-filesystem", 
        "/workspace/safe-directory",
        "--allowed-operations", "read"
      ]
    }
  ]
}
```

## Testing MCP Integration

### Unit Tests (Future)

```csharp
[Fact]
public async Task McpToolProvider_ShouldDiscoverTools()
{
    // Arrange
    var mockMcpServer = CreateMockMcpServer();
    var provider = new McpToolProvider(config, mockMcpServer);
    
    // Act
    await provider.InitializeAsync();
    var tools = provider.GetAllTools();
    
    // Assert
    Assert.Contains(tools, t => t.Name == "ReadFile");
}
```

### Integration Tests

```bash
# Start test MCP server
npx @modelcontextprotocol/server-filesystem /tmp/test-workspace &

# Run integration tests
dotnet test --filter Category=McpIntegration

# Cleanup
kill $MCP_SERVER_PID
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                  AIAgent                            │
│  (created with tools from ToolFactory)              │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
         ┌─────────────────┐
         │   ToolFactory   │
         └────────┬────────┘
                  │
        ┌─────────┴──────────┐
        │                    │
        ▼                    ▼
┌──────────────┐    ┌──────────────────┐
│LocalToolProv │    │ McpToolProvider  │
│              │    │                  │
│ Discovers C# │    │ Connects to MCP  │
│ methods with │    │ servers via:     │
│ [Description]│    │ - stdio          │
│              │    │ - HTTP           │
└──────────────┘    └────────┬─────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
                    ▼                 ▼
            ┌───────────────┐  ┌────────────────┐
            │ MCP Server 1  │  │  MCP Server 2  │
            │ (filesystem)  │  │  (github API)  │
            └───────────────┘  └────────────────┘
```

## Benefits of MCP Integration

1. **Standardized Interface**: Consistent tool interface across providers
2. **Ecosystem**: Access to growing library of official and community MCP servers
3. **Language Agnostic**: MCP servers can be written in any language
4. **Hot Reload**: Connect/disconnect MCP servers without recompiling
5. **Sandboxing**: Isolate tool execution in separate processes
6. **Discovery**: Dynamically discover available tools from servers

## Migration Path

### Current: Local Tools Only

```yaml
tools:
  - GetCurrentWeather  # C# static method
  - CalculateSum       # C# static method
```

### Future: Hybrid Local + MCP

```yaml
tools:
  - GetCurrentWeather  # Local C# tool
  - ReadFile           # MCP filesystem server
  - QueryGitHub        # MCP GitHub server
```

### Ultimate: Pure MCP

```yaml
tools:
  - weather.current    # MCP weather server
  - fs.read            # MCP filesystem server
  - db.query           # MCP database server
  - api.github.search  # MCP GitHub server
```

## Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [MCP C# SDK on GitHub](https://github.com/modelcontextprotocol/csharp-sdk)
- [Official MCP Servers](https://github.com/modelcontextprotocol/servers)
- [Microsoft Learn: MCP with .NET](https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp)

## Contributing

To implement MCP support:

1. Add ModelContextProtocol NuGet packages
2. Implement MCP client initialization in `McpToolProvider`
3. Add tool discovery logic
4. Create AITool wrappers for MCP tools
5. Write unit and integration tests
6. Update this documentation with actual implementation details

## Timeline

- **Q1 2026**: Initial MCP SDK integration
- **Q2 2026**: Support for official MCP servers (filesystem, GitHub, etc.)
- **Q3 2026**: Custom MCP server creation tools
- **Q4 2026**: Advanced features (hot reload, server health monitoring)

---

**Last Updated**: 2026-01-30  
**Status**: Planning / Placeholder Implementation  
**Version**: 0.1.0 (Pre-release)
