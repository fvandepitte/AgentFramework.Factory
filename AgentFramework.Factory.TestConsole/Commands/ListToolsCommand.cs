using AgentFramework.Factory.TestConsole.Services.Factories;
using AgentFramework.Factory.TestConsole.Services.Tools;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AgentFramework.Factory.TestConsole.Commands;

/// <summary>
/// Command to list all discovered tools
/// </summary>
public class ListToolsCommand : Command<ListToolsCommand.Settings>
{
    private readonly ToolFactory _toolFactory;

    public ListToolsCommand(ToolFactory toolFactory)
    {
        _toolFactory = toolFactory ?? throw new ArgumentNullException(nameof(toolFactory));
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-p|--provider <PROVIDER>")]
        public string? Provider { get; set; }

        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            AnsiConsole.Write(
                new FigletText("Tools")
                    .LeftJustified()
                    .Color(Color.Green));

            AnsiConsole.WriteLine();

            // Get tool providers
            var providers = _toolFactory.GetProviders().ToList();

            if (!string.IsNullOrEmpty(settings.Provider))
            {
                var provider = _toolFactory.GetProvider(settings.Provider);
                if (provider == null)
                {
                    AnsiConsole.MarkupLine($"[red]Provider '{settings.Provider}' not found[/]");
                    return 1;
                }
                providers = new List<IToolProvider> { provider };
            }

            // Display provider summary
            var providerTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Provider")
                .AddColumn("Type")
                .AddColumn("Tools");

            foreach (var provider in providers)
            {
                var tools = provider.GetAllTools().ToList();
                providerTable.AddRow(
                    $"[bold]{provider.Name}[/]",
                    provider.Type,
                    tools.Count.ToString()
                );
            }

            AnsiConsole.Write(providerTable);
            AnsiConsole.WriteLine();

            // Display detailed tool information
            foreach (var provider in providers)
            {
                var tools = provider.GetAllTools().ToList();
                
                if (!tools.Any())
                {
                    AnsiConsole.MarkupLine($"[dim]No tools available from {provider.Name} provider[/]");
                    continue;
                }

                AnsiConsole.Write(new Rule($"[blue]{provider.Name} Provider Tools[/]")
                    .LeftJustified());
                AnsiConsole.WriteLine();

                var toolTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Tool Name")
                    .AddColumn("Type");

                foreach (var tool in tools)
                {
                    // AIFunction implements AITool
                    var aiFunction = tool as Microsoft.Extensions.AI.AIFunction;
                    var name = aiFunction?.Name ?? "Unknown";
                    var type = tool.GetType().Name;

                    toolTable.AddRow(
                        $"[cyan]{name}[/]",
                        type
                    );
                }

                AnsiConsole.Write(toolTable);
                AnsiConsole.WriteLine();

                // Show detailed info if verbose
                if (settings.Verbose)
                {
                    foreach (var tool in tools)
                    {
                        var aiFunction = tool as Microsoft.Extensions.AI.AIFunction;
                        if (aiFunction == null) continue;
                        
                        var panel = new Panel(new Markup(
                            $"[bold]Name:[/] {aiFunction.Name}\n" +
                            $"[bold]Type:[/] {tool.GetType().Name}"
                        ))
                        {
                            Header = new PanelHeader($"[yellow]{aiFunction.Name}[/]"),
                            Border = BoxBorder.Rounded
                        };
                        AnsiConsole.Write(panel);
                    }
                }
            }

            AnsiConsole.MarkupLine($"[green]✓[/] Total providers: {providers.Count}");
            AnsiConsole.MarkupLine($"[green]✓[/] Total tools: {providers.SelectMany(p => p.GetAllTools()).Count()}");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
