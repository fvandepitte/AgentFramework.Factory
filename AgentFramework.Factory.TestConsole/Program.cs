using AgentFramework.Factory.TestConsole.Commands;
using AgentFramework.Factory.TestConsole.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

// Setup dependency injection
var services = new ServiceCollection();
services.AddAgentFactoryServices();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("agent-factory");
    
    config.AddCommand<ListCommand>("list")
        .WithDescription("List all available agents")
        .WithExample(new[] { "list" })
        .WithExample(new[] { "list", "--verbose" });
    
    config.AddCommand<ListToolsCommand>("list-tools")
        .WithDescription("List all discovered tools")
        .WithExample(new[] { "list-tools" })
        .WithExample(new[] { "list-tools", "--verbose" })
        .WithExample(new[] { "list-tools", "--provider", "local" });
    
    config.AddCommand<ShowCommand>("show")
        .WithDescription("Show details of a specific agent")
        .WithExample(new[] { "show", "WeatherAssistant" });
    
    config.AddCommand<TestAgentCommand>("test-agent")
        .WithDescription("Create and test an agent in an interactive chat session")
        .WithExample(new[] { "test-agent", "WeatherAssistant" });
});

return app.Run(args);
