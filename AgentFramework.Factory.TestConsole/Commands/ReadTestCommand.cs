using System.ComponentModel;
using AgentFramework.Factory.TestConsole.Services.Factories;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AgentFramework.Factory.TestConsole.Commands;

/// <summary>
/// Command to test reading and parsing a markdown agent file
/// </summary>
public class ReadTestCommand : Command<ReadTestCommand.Settings>
{
    private readonly MarkdownAgentFactory _factory;

    public ReadTestCommand(MarkdownAgentFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public class Settings : CommandSettings
    {
        [Description("Path to the markdown file to test")]
        [CommandArgument(0, "<markdown-file>")]
        public string MarkdownPath { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!File.Exists(settings.MarkdownPath))
        {
            AnsiConsole.MarkupLine($"[red]✗[/] File not found: [yellow]{settings.MarkdownPath}[/]");
            return 1;
        }

        AnsiConsole.Write(new FigletText("Read Test").Color(Color.Green));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[grey]Reading:[/] [yellow]{settings.MarkdownPath}[/]");
        AnsiConsole.WriteLine();

        try
        {
            var agent = AnsiConsole.Status()
                .Start("Parsing markdown file...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    return _factory.LoadAgentFromFile(settings.MarkdownPath);
                });

            AnsiConsole.MarkupLine("[green]✓[/] Successfully parsed agent definition");
            AnsiConsole.WriteLine();

            // Show parsed metadata
            var metadataTable = new Table();
            metadataTable.Border(TableBorder.Rounded);
            metadataTable.AddColumn("[blue]Property[/]");
            metadataTable.AddColumn("[blue]Value[/]");

            metadataTable.AddRow("[cyan]Name[/]", $"[green]{agent.Name}[/]");
            metadataTable.AddRow("[cyan]Description[/]", agent.Description);
            metadataTable.AddRow("[cyan]Model[/]", $"[yellow]{agent.Model}[/]");
            metadataTable.AddRow("[cyan]Temperature[/]", agent.Temperature.ToString("0.0"));
            metadataTable.AddRow("[cyan]Provider[/]", agent.Provider);
            
            if (agent.MaxTokens.HasValue)
                metadataTable.AddRow("[cyan]Max Tokens[/]", agent.MaxTokens.Value.ToString());
            
            if (agent.TopP.HasValue)
                metadataTable.AddRow("[cyan]Top P[/]", agent.TopP.Value.ToString("0.00"));
            
            if (agent.FrequencyPenalty.HasValue)
                metadataTable.AddRow("[cyan]Frequency Penalty[/]", agent.FrequencyPenalty.Value.ToString("0.00"));
            
            if (agent.PresencePenalty.HasValue)
                metadataTable.AddRow("[cyan]Presence Penalty[/]", agent.PresencePenalty.Value.ToString("0.00"));

            metadataTable.AddRow("[cyan]Instructions Length[/]", $"{agent.Instructions.Length} characters");

            AnsiConsole.Write(new Panel(metadataTable)
            {
                Header = new PanelHeader("[blue]Parsed Metadata[/]"),
                Border = BoxBorder.Double
            });

            AnsiConsole.WriteLine();

            // Show instructions preview
            var instructionsPreview = agent.Instructions.Length > 500
                ? agent.Instructions.Substring(0, 500) + "..."
                : agent.Instructions;

            var instructionsPanel = new Panel(instructionsPreview)
            {
                Header = new PanelHeader("[blue]Instructions Preview[/]"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(instructionsPanel);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]✗ Error parsing markdown file[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
