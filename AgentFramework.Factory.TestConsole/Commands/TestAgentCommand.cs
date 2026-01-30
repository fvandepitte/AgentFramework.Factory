using Spectre.Console;
using Spectre.Console.Cli;
using AgentFramework.Factory.TestConsole.Services;

namespace AgentFramework.Factory.TestConsole.Commands;

public class TestAgentCommand : Command<TestAgentCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[agentName]")]
        public string? AgentName { get; set; }

        [CommandOption("-c|--config <path>")]
        public string ConfigFile { get; set; } = "appsettings.json";

        [CommandOption("-m|--message <text>")]
        public string? Message { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            // Load configuration
            var config = ConfigurationLoader.LoadConfiguration(settings.ConfigFile);
            
            // Create agent factory
            var agentFactory = new AgentFactory(config);

            AnsiConsole.Write(
                new FigletText("Agent Test")
                    .LeftJustified()
                    .Color(Color.Blue));

            AnsiConsole.WriteLine();

            string agentName;
            if (string.IsNullOrEmpty(settings.AgentName))
            {
                // List available agents and let user choose
                var agents = config.Agents.Where(a => a.Enabled).ToList();
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
            var (isValid, errorMessage) = agentFactory.ValidateAgent(agentName);
            if (!isValid)
            {
                AnsiConsole.MarkupLine($"[red]Agent validation failed: {errorMessage}[/]");
                return 1;
            }

            // Create the agent
            AnsiConsole.Status()
                .Start("Creating agent...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    var agent = agentFactory.CreateAgentByName(agentName);
                    
                    AnsiConsole.MarkupLine($"[green]✓[/] Agent created: [bold]{agent.Name}[/]");
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Agent is ready to use![/]");
            AnsiConsole.WriteLine();

            // If message provided, test with that message
            if (!string.IsNullOrEmpty(settings.Message))
            {
                TestAgentWithMessageAsync(agentFactory, agentName, settings.Message).Wait();
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]Use --message to test the agent with a prompt[/]");
                AnsiConsole.MarkupLine("[dim]Example: dotnet run -- test-agent WeatherAssistant --message \"Hello!\"[/]");
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private async Task TestAgentWithMessageAsync(AgentFactory factory, string agentName, string message)
    {
        try
        {
            var agent = factory.CreateAgentByName(agentName);

            AnsiConsole.MarkupLine($"[yellow]Testing agent with message:[/] {message}");
            AnsiConsole.WriteLine();

            await AnsiConsole.Status()
                .StartAsync("Getting response...", async ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("yellow"));

                    // Create a new session for the agent
                    var session = await agent.GetNewSessionAsync();

                    // Get agent response
                    var response = await agent.RunAsync(message, session);

                    AnsiConsole.WriteLine();
                    var panel = new Panel(response.ToString())
                    {
                        Header = new PanelHeader($"[bold]{agentName} Response[/]"),
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(Color.Green)
                    };
                    AnsiConsole.Write(panel);
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]Error testing agent: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[dim]{ex.InnerException.Message}[/]");
            }
        }
    }
}
