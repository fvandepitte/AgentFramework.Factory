# CLI Commands Guide

The Agent Framework Markdown Factory now uses **Spectre.Console.Cli** for a beautiful command-line interface.

## Available Commands

### `list` - List All Agents

Lists all agents defined in the configuration.

```bash
# Basic list
dotnet run -- list

# Verbose mode (shows source files and providers)
dotnet run -- list --verbose
dotnet run -- list -v

# With custom config file
dotnet run -- list --config appsettings.Development.json
```

**Example Output:**
```
     _                             _       _       _         _   
    / \      __ _    ___   _ __   | |_    | |     (_)  ___  | |_ 
   / _ \    / _` |  / _ \ | '_ \  | __|   | |     | | / __| | __|
  / ___ \  | (_| | |  __/ | | | | | |_    | |___  | | \__ \ | |_ 
 /_/   \_\  \__, |  \___| |_| |_|  \__|   |_____| |_| |___/  \__|
            |___/                                                

╭──────────────────┬───────────────────────────┬─────────────┬──────╮
│ Name             │ Description               │ Model       │ Temp │
├──────────────────┼───────────────────────────┼─────────────┼──────┤
│ WeatherAssistant │ Weather info assistant    │ gpt-4o-mini │ 0.7  │
╰──────────────────┴───────────────────────────┴─────────────┴──────╯
```

---

### `show` - Show Agent Details

Display detailed information about a specific agent.

```bash
# Show agent details
dotnet run -- show WeatherAssistant

# With custom config
dotnet run -- show WeatherAssistant --config appsettings.Development.json
```

**Example Output:**
```
─────────────────────────────── WeatherAssistant ───────────────────────────────

Description:     A helpful weather assistant
Model:           gpt-4o-mini
Temperature:     0.7
Provider:        azureOpenAI
Source File:     ./sample-agent.md

╭─Instructions─────────────────────────────────────────────────╮
│ # Persona                                                    │
│                                                              │
│ You are a friendly weather assistant...                     │
╰──────────────────────────────────────────────────────────────╯
```

---

### `read-test` - Test Markdown Parsing

Test reading and parsing a markdown agent file directly.

```bash
# Test parsing a markdown file
dotnet run -- read-test sample-agent.md

# Test with custom markdown file
dotnet run -- read-test ./agents/my-agent.md

# With custom config
dotnet run -- read-test sample-agent.md --config appsettings.json
```

**Example Output:**
```
  ____                       _     _____                _   
 |  _ \    ___    __ _    __| |   |_   _|   ___   ___  | |_ 
 | |_) |  / _ \  / _` |  / _` |     | |    / _ \ / __| | __|
 |  _ <  |  __/ | (_| | | (_| |     | |   |  __/ \__ \ | |_ 
 |_| \_\  \___|  \__,_|  \__,_|     |_|    \___| |___/  \__|

Reading: sample-agent.md

✓ Successfully parsed agent definition

╔═Parsed Metadata══════════════════════════════════╗
║ ╭─────────────────────┬─────────────────────────╮ ║
║ │ Property            │ Value                   │ ║
║ ├─────────────────────┼─────────────────────────┤ ║
║ │ Name                │ WeatherAssistant        │ ║
║ │ Description         │ Weather info assistant  │ ║
║ │ Model               │ gpt-4o-mini             │ ║
║ │ Temperature         │ 0.7                     │ ║
║ │ Provider            │ azureOpenAI             │ ║
║ │ Instructions Length │ 1424 characters         │ ║
║ ╰─────────────────────┴─────────────────────────╯ ║
╚══════════════════════════════════════════════════╝
```

---

### `interactive` (or `i`) - Interactive Explorer

Launch an interactive agent explorer with a selection menu.

```bash
# Start interactive mode
dotnet run -- interactive

# Or use the short alias
dotnet run -- i

# With custom config
dotnet run -- interactive --config appsettings.json
```

**Features:**
- Beautiful selection menu
- Navigate with arrow keys
- View full agent details
- No typing required!

**Example Flow:**
```
     _                             _       _____            _                       
    / \      __ _    ___   _ __   | |_    | ____|  __  __  | |__    
   / _ \    / _` |  / _ \ | '_ \  | __|   |  _|    \ \/ /  | '_ \  
  / ___ \  | (_| | |  __/ | | | | | |_    | |___    >  <   | |_) |
 /_/   \_\  \__, |  \___| |_| |_|  \__|   |_____|  /_/\_\  |_.__/  
            |___/                                                  

✓ Loaded 3 agent(s)

? Select an agent to explore:
❯ WeatherAssistant - A helpful weather assistant
  CodeReviewer - Reviews code and suggests improvements  
  Translator - Translates text between languages
  
(Move up and down to reveal more agents)
```

---

## Global Options

All commands support these global options:

- `-c|--config <path>` - Specify custom configuration file
- `-h|--help` - Show help for the command

---

## Command Structure

```
agent-factory <command> [arguments] [options]
```

### Examples

```bash
# Get general help
dotnet run -- --help

# Get help for specific command
dotnet run -- list --help
dotnet run -- show --help
dotnet run -- read-test --help

# Chain commands in scripts
dotnet run -- list --verbose > agents.txt
dotnet run -- show WeatherAssistant > weather-agent.txt
```

---

## Build as Standalone Executable

You can publish the application as a standalone executable:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

Then use without `dotnet run`:

```bash
# Windows
.\agent-factory.exe list

# Linux/macOS
./agent-factory list
```

---

## Advanced Usage

### Custom Configuration Per Environment

```bash
# Development
dotnet run -- list --config appsettings.Development.json

# Production
dotnet run -- list --config appsettings.Production.json

# Local overrides (not in git)
dotnet run -- list --config appsettings.Local.json
```

### Scripting

```bash
# PowerShell - Export all agent details
$agents = dotnet run -- list --verbose
foreach ($agent in $agents) {
    dotnet run -- show $agent > "exports/$agent.txt"
}
```

```bash
# Bash - Test all markdown files
for file in agents/*.md; do
    dotnet run -- read-test "$file"
done
```

---

## Tips & Tricks

1. **Use tab completion** - The CLI auto-completes commands (if supported by your shell)

2. **Alias for quick access** - Create a shell alias:
   ```bash
   # PowerShell
   Set-Alias af 'dotnet run --'
   
   # Then use:
   af list
   af show WeatherAssistant
   ```

3. **JSON output** - Currently outputs formatted tables. For JSON output, add a `--json` flag (TODO)

4. **Colors** - Spectre.Console automatically detects terminal capabilities and adjusts colors

---

## Architecture

The CLI uses **Spectre.Console.Cli** which provides:
- ✅ Beautiful ASCII art and formatting
- ✅ Automatic help generation
- ✅ Type-safe command definitions
- ✅ Rich progress indicators and spinners
- ✅ Interactive prompts and menus
- ✅ Colored, styled output

Each command is a separate class in `Commands/`:
- `ListCommand.cs` - Lists agents
- `ShowCommand.cs` - Shows agent details  
- `ReadTestCommand.cs` - Tests markdown parsing
- `InteractiveCommand.cs` - Interactive explorer

Services in `Services/`:
- `ConfigurationLoader.cs` - Loads app configuration
- `MarkdownAgentFactory.cs` - Parses markdown files
- `Configuration.cs` - Configuration models

---

**Last Updated**: 2026-01-30
