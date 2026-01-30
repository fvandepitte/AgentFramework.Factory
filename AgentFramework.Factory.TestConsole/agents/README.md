# GitHub DevOps Agent - Sample MCP Integration

This agent demonstrates the planned integration with the GitHub Model Context Protocol (MCP) server for DevOps automation and CI/CD monitoring.

## Overview

The **GitHubDevOps** agent is a sample implementation that showcases:
- Agent definition using markdown with YAML frontmatter
- GitHub MCP server tool configuration
- Mock/placeholder tools matching GitHub MCP API signatures
- DevOps-focused persona for CI/CD automation

## Files Created

### 1. Agent Definition
**Location**: `agents/github-devops.md`

Markdown file with:
- YAML frontmatter defining agent metadata (name, model, temperature, tools)
- Detailed persona and responsibilities
- Example interactions and boundaries
- Response formatting guidelines

**Tools configured**:
- `list_workflow_runs` - Monitor GitHub Actions executions
- `get_workflow_run_details` - Analyze specific workflow runs
- `get_repository_content` - Access repository files/configs
- `list_pull_requests` - Track PR status
- `create_issue` - Report workflow failures
- `list_commits` - Analyze commit history

### 2. Mock Tools Implementation
**Location**: `Tools/Samples/GitHubMockTools.cs`

Placeholder tools that simulate GitHub MCP server responses:
- Match the expected GitHub MCP API signatures
- Return realistic mock data for testing
- Include warnings that real MCP integration is pending
- Registered in DI container for automatic discovery

### 3. Configuration Updates
**Location**: `appsettings.json`

Added:
```json
{
  "mcpServers": [
    {
      "name": "github",
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/",
      "environment": {}
    }
  ],
  "agents": [
    {
      "name": "GitHubDevOps",
      "enabled": true,
      "markdownPath": "./agents/github-devops.md",
      "overrides": {
        "temperature": 0.3,
        "maxTokens": 3000
      }
    }
  ]
}
```

## Current Status

### ✅ Implemented
- Agent markdown definition with comprehensive persona
- Mock tools matching GitHub MCP signatures
- Configuration for remote GitHub MCP server
- DI registration for tool discovery
- Build and configuration loading verified

### ⏳ Pending (MCP SDK Integration Required)
- Actual GitHub API calls via MCP server
- OAuth/PAT authentication
- Real-time workflow monitoring
- Live repository access
- Issue creation on GitHub

## Testing

### List Agents
```bash
dotnet run -- list
```

### Show Agent Details
```bash
dotnet run -- show GitHubDevOps
```

### List Available Tools
```bash
dotnet run -- list-tools
```

### Test Agent (Interactive)
```bash
dotnet run -- test-agent GitHubDevOps
```

Sample queries to try:
- "Show me recent workflow runs for microsoft/agent-framework"
- "What pull requests are open in my repository?"
- "Get the CI/CD configuration from .github/workflows/"
- "List recent commits"

## Mock Tool Responses

The current implementation returns **realistic mock data** that demonstrates what the agent will do once MCP integration is complete:

- Workflow runs show success/failure status, timing, branches
- Workflow details include step-by-step execution logs
- Repository content shows YAML configs and directory listings
- Pull requests display author, status, reviews, file changes
- Issues show what would be created with labels and assignments
- Commits display author, timestamp, file changes

All responses include a warning: `[Mock data - MCP integration pending]`

## Migration to Real MCP

When the MCP SDK is integrated:

1. **Enable MCP in configuration**:
   ```json
   "tools": {
     "enableMcp": true
   }
   ```

2. **McpToolProvider implementation** will:
   - Connect to `https://api.githubcopilot.com/mcp/`
   - Discover available GitHub tools
   - Create AITool wrappers
   - Route tool calls to GitHub API

3. **Mock tools will be automatically bypassed** via Chain of Responsibility pattern:
   - MCP provider gets priority
   - Local tools serve as fallback

4. **No changes needed** to agent markdown or configuration!

## Architecture Pattern

This implementation demonstrates the **Chain of Responsibility** pattern for tool resolution:

```
Agent requests tool → ToolFactory
                          ↓
                    McpToolProvider (priority)
                          ↓ (if MCP enabled and available)
                    LocalToolProvider (fallback)
                          ↓
                    GitHubMockTools.* methods
```

Once MCP is integrated, the chain will prefer MCP tools, with local mocks as fallback for offline development.

## Future Enhancements

1. **Authentication**: Add PAT support for private repositories
2. **Toolsets**: Enable specific GitHub MCP toolsets (actions, security, dependabot)
3. **Read-only mode**: Ensure safe operations during testing
4. **Error handling**: Graceful degradation when GitHub API is unavailable
5. **Multiple agents**: Create specialized agents (CodeReview, IssueTriage, Analytics)

## Related Documentation

- [MCP Integration Guide](../Copilot_Summaries/Tools/MCP.md)
- [Tools Overview](../Copilot_Summaries/Tools/TOOLS.md)
- [Chain of Responsibility Pattern](../Copilot_Summaries/Chain_Of_Responsibility/CHAIN_OF_RESPONSIBILITY.md)
- [Configuration Guide](../Copilot_Summaries/Configuration/CONFIG.md)

---

**Created**: January 30, 2026  
**Status**: Mock implementation ready, awaiting MCP SDK integration
