using AgentFramework.Factory.TestConsole.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("agent-factory");
    
    config.AddCommand<ListCommand>("list")
        .WithDescription("List all available agents")
        .WithExample(new[] { "list" })
        .WithExample(new[] { "list", "--verbose" });
    
    config.AddCommand<ShowCommand>("show")
        .WithDescription("Show details of a specific agent")
        .WithExample(new[] { "show", "WeatherAssistant" });
    
    config.AddCommand<ReadTestCommand>("read-test")
        .WithDescription("Test reading and parsing a markdown agent file")
        .WithExample(new[] { "read-test", "sample-agent.md" });
    
    config.AddCommand<InteractiveCommand>("interactive")
        .WithDescription("Interactive agent explorer")
        .WithAlias("i")
        .WithExample(new[] { "interactive" });
    
    config.AddCommand<TestAgentCommand>("test-agent")
        .WithDescription("Create and test an agent")
        .WithExample(new[] { "test-agent", "WeatherAssistant" })
        .WithExample(new[] { "test-agent", "WeatherAssistant", "--message", "Hello!" });
});

return app.Run(args);
