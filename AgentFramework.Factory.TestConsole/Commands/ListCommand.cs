using System.ComponentModel;
using AgentFramework.Factory.TestConsole.Services;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AgentFramework.Factory.TestConsole.Commands;

/// <summary>
/// Command to list all available agents from markdown files
/// </summary>
public class ListCommand : Command<ListCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to configuration file")]
        [CommandOption("-c|--config")]
        public string? ConfigPath { get; set; }

        [Description("Show detailed information")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var config = ConfigurationLoader.LoadConfiguration(settings.ConfigPath);
        var factory = new MarkdownAgentFactory(config);

        AnsiConsole.Write(new FigletText("Agent List").Color(Color.Blue));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[grey]Agent Definitions Path:[/] [yellow]{config.AgentFactory.AgentDefinitionsPath}[/]");
        AnsiConsole.MarkupLine($"[grey]File Pattern:[/] [yellow]{config.AgentFactory.AgentFilePattern}[/]");
        AnsiConsole.MarkupLine($"[grey]Default Provider:[/] [yellow]{config.AgentFactory.DefaultProvider}[/]");
        AnsiConsole.WriteLine();

        var agents = AnsiConsole.Status()
            .Start("Loading agents...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                return factory.LoadAgentsFromConfiguration();
            });

        if (agents.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠[/] No agents found.");
            return 0;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[blue]Name[/]");
        table.AddColumn("[blue]Description[/]");
        table.AddColumn("[blue]Model[/]");
        table.AddColumn("[blue]Temp[/]");
        
        if (settings.Verbose)
        {
            table.AddColumn("[blue]Source File[/]");
            table.AddColumn("[blue]Provider[/]");
        }

        foreach (var agent in agents)
        {
            if (settings.Verbose)
            {
                table.AddRow(
                    $"[green]{agent.Name}[/]",
                    agent.Description,
                    $"[cyan]{agent.Model}[/]",
                    agent.Temperature.ToString("0.0"),
                    $"[grey]{agent.SourceFile}[/]",
                    $"[yellow]{agent.Provider}[/]"
                );
            }
            else
            {
                table.AddRow(
                    $"[green]{agent.Name}[/]",
                    agent.Description,
                    $"[cyan]{agent.Model}[/]",
                    agent.Temperature.ToString("0.0")
                );
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]✓[/] Loaded [bold]{agents.Count}[/] agent(s)");

        return 0;
    }
}
