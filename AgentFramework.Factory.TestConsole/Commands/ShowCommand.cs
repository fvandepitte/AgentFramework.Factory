using System.ComponentModel;
using AgentFramework.Factory.TestConsole.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AgentFramework.Factory.TestConsole.Commands;

/// <summary>
/// Command to show details of a specific agent
/// </summary>
public class ShowCommand : Command<ShowCommand.Settings>
{
    private readonly MarkdownAgentFactory _factory;

    public ShowCommand(MarkdownAgentFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public class Settings : CommandSettings
    {
        [Description("Name of the agent to show")]
        [CommandArgument(0, "<agent-name>")]
        public string AgentName { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var agents = _factory.LoadAgentsFromConfiguration();
        var agent = agents.FirstOrDefault(a => 
            a.Name.Equals(settings.AgentName, StringComparison.OrdinalIgnoreCase));

        if (agent == null)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Agent '[yellow]{settings.AgentName}[/]' not found.");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Available agents:[/]");
            foreach (var a in agents)
            {
                AnsiConsole.MarkupLine($"  [green]•[/] {a.Name}");
            }
            return 1;
        }

        AnsiConsole.Write(new Rule($"[blue]{agent.Name}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow("[grey]Description:[/]", agent.Description);
        grid.AddRow("[grey]Model:[/]", $"[cyan]{agent.Model}[/]");
        grid.AddRow("[grey]Temperature:[/]", agent.Temperature.ToString("0.0"));
        grid.AddRow("[grey]Provider:[/]", $"[yellow]{agent.Provider}[/]");
        grid.AddRow("[grey]Source File:[/]", $"[grey]{agent.SourceFile}[/]");
        
        if (agent.MaxTokens.HasValue)
            grid.AddRow("[grey]Max Tokens:[/]", agent.MaxTokens.Value.ToString());
        
        if (agent.TopP.HasValue)
            grid.AddRow("[grey]Top P:[/]", agent.TopP.Value.ToString("0.00"));

        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        var panel = new Panel(agent.Instructions)
        {
            Header = new PanelHeader("[blue]Instructions[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);

        return 0;
    }
}
