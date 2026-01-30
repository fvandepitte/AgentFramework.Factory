using System.ComponentModel;
using AgentFramework.Factory.TestConsole.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AgentFramework.Factory.TestConsole.Commands;

/// <summary>
/// Interactive command to select and view agent details
/// </summary>
public class InteractiveCommand : Command<InteractiveCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to configuration file")]
        [CommandOption("-c|--config")]
        public string? ConfigPath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var config = ConfigurationLoader.LoadConfiguration(settings.ConfigPath);
        var factory = new MarkdownAgentFactory(config);

        AnsiConsole.Write(new FigletText("Agent Explorer").Color(Color.Cyan1));
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

        AnsiConsole.MarkupLine($"[green]✓[/] Loaded [bold]{agents.Count}[/] agent(s)");
        AnsiConsole.WriteLine();

        var selectedAgent = AnsiConsole.Prompt(
            new SelectionPrompt<LoadedAgent>()
                .Title("Select an agent to explore:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more agents)[/]")
                .AddChoices(agents)
                .UseConverter(agent => $"{agent.Name} - {agent.Description}")
        );

        AnsiConsole.Clear();
        
        AnsiConsole.Write(new Rule($"[cyan]{selectedAgent.Name}[/]").RuleStyle("blue"));
        AnsiConsole.WriteLine();

        var infoGrid = new Grid();
        infoGrid.AddColumn();
        infoGrid.AddColumn();

        infoGrid.AddRow("[grey]Description:[/]", selectedAgent.Description);
        infoGrid.AddRow("[grey]Model:[/]", $"[cyan]{selectedAgent.Model}[/]");
        infoGrid.AddRow("[grey]Temperature:[/]", selectedAgent.Temperature.ToString("0.0"));
        infoGrid.AddRow("[grey]Provider:[/]", $"[yellow]{selectedAgent.Provider}[/]");
        infoGrid.AddRow("[grey]Source File:[/]", $"[grey]{selectedAgent.SourceFile}[/]");
        
        if (selectedAgent.MaxTokens.HasValue)
            infoGrid.AddRow("[grey]Max Tokens:[/]", selectedAgent.MaxTokens.Value.ToString());

        AnsiConsole.Write(infoGrid);
        AnsiConsole.WriteLine();

        var panel = new Panel(selectedAgent.Instructions)
        {
            Header = new PanelHeader("[cyan]Instructions[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Markup("[grey]Press any key to exit...[/]"));
        Console.ReadKey(true);

        return 0;
    }
}
