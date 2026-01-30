using AgentFramework.Factory.TestConsole.Services.Configuration;
using AgentFramework.Factory.TestConsole.Services.Factories;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AgentFramework.Factory.TestConsole.Commands;

public class TestAgentCommand : Command<TestAgentCommand.Settings>
{
    private readonly AgentFactory _agentFactory;
    private readonly AppConfiguration _config;

    public TestAgentCommand(AgentFactory agentFactory, IOptions<AppConfiguration> config)
    {
        _agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
        ArgumentNullException.ThrowIfNull(config);
        _config = config.Value;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[agentName]")]
        public string? AgentName { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            AnsiConsole.Write(
                new FigletText("Agent Test")
                    .LeftJustified()
                    .Color(Color.Blue));

            AnsiConsole.WriteLine();

            string agentName;
            if (string.IsNullOrEmpty(settings.AgentName))
            {
                // List available agents and let user choose
                var agents = _config.Agents.Where(a => a.Enabled).ToList();
                if (!agents.Any())
                {
                    AnsiConsole.MarkupLine("[red]No enabled agents found in configuration[/]");
                    return 1;
                }

                agentName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an agent to test:")
                        .AddChoices(agents.Select(a => a.Name)));
            }
            else
            {
                agentName = settings.AgentName;
            }

            // Validate agent
            var (isValid, errorMessage) = _agentFactory.ValidateAgent(agentName);
            if (!isValid)
            {
                AnsiConsole.MarkupLine($"[red]Agent validation failed: {errorMessage}[/]");
                return 1;
            }

            // Create the agent
            var agent = AnsiConsole.Status()
                .Start("Creating agent...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    var createdAgent = _agentFactory.CreateAgentByName(agentName);
                    
                    AnsiConsole.MarkupLine($"[green]✓[/] Agent created: [bold]{createdAgent.Name}[/]");
                    return createdAgent;
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Agent is ready! Starting interactive chat session...[/]");
            AnsiConsole.MarkupLine("[dim]Type 'exit' or 'quit' to end the session[/]");
            AnsiConsole.WriteLine();

            // Start interactive chat session
            RunInteractiveChatAsync(agent, agentName, cancellationToken).Wait();

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private async Task RunInteractiveChatAsync(Microsoft.Agents.AI.AIAgent agent, string agentName, CancellationToken cancellationToken)
    {
        try
        {
            // Create a new session for the agent
            var session = await agent.GetNewSessionAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                // Get user input
                var userMessage = AnsiConsole.Ask<string>("[bold blue]You:[/]");

                // Check for exit commands
                if (string.Equals(userMessage.Trim(), "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(userMessage.Trim(), "quit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[yellow]Ending chat session. Goodbye![/]");
                    break;
                }

                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    continue;
                }

                // Get agent response
                await AnsiConsole.Status()
                    .StartAsync("Thinking...", async ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(Style.Parse("yellow"));

                        var response = await agent.RunAsync(userMessage, session);

                        AnsiConsole.WriteLine();
                        // Escape markup to prevent Spectre from interpreting brackets in the response
                        var escapedResponse = Markup.Escape(response.ToString() ?? string.Empty);
                        var panel = new Panel(escapedResponse)
                        {
                            Header = new PanelHeader($"[bold green]{agentName}:[/]"),
                            Border = BoxBorder.Rounded,
                            BorderStyle = new Style(Color.Green)
                        };
                        AnsiConsole.Write(panel);
                        AnsiConsole.WriteLine();
                    });
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]Error in chat session: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[dim]{ex.InnerException.Message}[/]");
            }
        }
    }
}
