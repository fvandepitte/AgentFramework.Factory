using Microsoft.Extensions.Configuration;

namespace AgentFramework.Factory.TestConsole.Services.Configuration;

/// <summary>
/// Service for loading application configuration
/// </summary>
public static class ConfigurationLoader
{
    public static AppConfiguration LoadConfiguration(string? configPath = null)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configPath ?? "appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        
        var appConfig = configuration.Get<AppConfiguration>();
        
        if (appConfig == null)
        {
            throw new InvalidOperationException("Failed to load configuration");
        }

        return appConfig;
    }
}
