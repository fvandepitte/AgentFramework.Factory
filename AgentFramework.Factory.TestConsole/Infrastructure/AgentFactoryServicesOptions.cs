using Microsoft.Extensions.Configuration;
using Serilog;

namespace AgentFramework.Factory.TestConsole.Infrastructure;

/// <summary>
/// Options for configuring AgentFactory services
/// </summary>
public class AgentFactoryServicesOptions
{
    /// <summary>
    /// Optional path to configuration file (defaults to appsettings.json)
    /// </summary>
    public string? ConfigFilePath { get; set; }

    /// <summary>
    /// Optional callback to customize configuration builder
    /// </summary>
    public Action<IConfigurationBuilder>? ConfigureConfiguration { get; set; }

    /// <summary>
    /// Optional callback to customize logger configuration
    /// </summary>
    public Action<LoggerConfiguration>? ConfigureLogging { get; set; }
}
