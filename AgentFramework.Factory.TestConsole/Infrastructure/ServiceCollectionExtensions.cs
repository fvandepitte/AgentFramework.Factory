using AgentFramework.Factory.TestConsole.Services.Configuration;
using AgentFramework.Factory.TestConsole.Services.Factories;
using AgentFramework.Factory.TestConsole.Services.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentFramework.Factory.TestConsole.Infrastructure;

/// <summary>
/// Service registration and configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services
    /// </summary>
    public static IServiceCollection AddAgentFactoryServices(
        this IServiceCollection services,
        string? configFilePath = null)
    {
        // Build configuration
        var configuration = BuildConfiguration(configFilePath);
        
        // Register IConfiguration
        services.AddSingleton<IConfiguration>(configuration);
        
        // Register logging
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Information);
        });

        // Register configuration models using Options pattern
        services.Configure<AppConfiguration>(configuration);
        services.Configure<AgentFactoryConfiguration>(configuration.GetSection("agentFactory"));
        services.Configure<ProvidersConfiguration>(configuration.GetSection("providers"));
        services.Configure<AzureOpenAIConfiguration>(configuration.GetSection("providers:azureOpenAI"));
        services.Configure<OpenAIConfiguration>(configuration.GetSection("providers:openAI"));
        services.Configure<GitHubModelsConfiguration>(configuration.GetSection("providers:githubModels"));

        // Register provider handlers
        services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();
        services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();
        services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();

        // Register services
        services.AddSingleton<MarkdownAgentFactory>();
        services.AddSingleton<ProviderFactory>();
        services.AddSingleton<AgentFactory>();

        return services;
    }

    /// <summary>
    /// Build the configuration from JSON files, environment variables, and user secrets
    /// </summary>
    private static IConfiguration BuildConfiguration(string? configFilePath)
    {
        var configPath = configFilePath ?? "appsettings.json";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configPath, optional: false, reloadOnChange: true);

        // Add environment-specific configuration
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                       ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (!string.IsNullOrEmpty(environment))
        {
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
        }

        // Add environment variables
        builder.AddEnvironmentVariables();

        // Add user secrets (if in development)
        if (environment == "Development" || string.IsNullOrEmpty(environment))
        {
            builder.AddUserSecrets<Program>(optional: true);
        }

        return builder.Build();
    }
}
