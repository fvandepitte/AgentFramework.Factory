using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Configuration;
using AgentFramework.Factory.Extensions;
using AgentFramework.Factory.Services;
using AgentFramework.Factory.Provider.AzureOpenAI.Extensions;
using AgentFramework.Factory.Provider.OpenAI.Extensions;
using AgentFramework.Factory.Provider.GitHubModels.Extensions;
using AgentFramework.Factory.TestConsole.Services.Configuration;
using AgentFramework.Factory.TestConsole.Tools.Samples;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using CoreMarkdownFactory = AgentFramework.Factory.Services.MarkdownAgentFactory;

namespace AgentFramework.Factory.TestConsole.Infrastructure;

/// <summary>
/// Service registration and configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services with optional configuration callback
    /// </summary>
    public static IServiceCollection AddAgentFactoryServices(
        this IServiceCollection services,
        string? configFilePath = null,
        Action<IConfigurationBuilder>? configureConfiguration = null,
        Action<LoggerConfiguration>? configureLogging = null)
    {
        // Build configuration with optional callback for customization
        var configuration = BuildConfiguration(configFilePath, configureConfiguration);
        
        // Register IConfiguration
        services.AddSingleton<IConfiguration>(configuration);
        
        // Configure logging with optional callback
        ConfigureSerilogLogging(services, configureLogging);

        // Register configuration models using Options pattern
        RegisterConfigurationOptions(services, configuration);

        // Register provider handlers
        RegisterProviderHandlers(services);

        // Register tool providers and tools
        RegisterToolProviders(services);

        // Register core factories
        RegisterFactories(services);

        return services;
    }

    /// <summary>
    /// Register all application services with detailed options callbacks for each configuration section
    /// </summary>
    public static IServiceCollection AddAgentFactoryServices(
        this IServiceCollection services,
        Action<AgentFactoryServicesOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);
        
        var options = new AgentFactoryServicesOptions();
        configureOptions(options);

        return AddAgentFactoryServices(
            services,
            options.ConfigFilePath,
            options.ConfigureConfiguration,
            options.ConfigureLogging);
    }

    /// <summary>
    /// Build the configuration from JSON files, environment variables, and user secrets
    /// </summary>
    private static IConfiguration BuildConfiguration(
        string? configFilePath, 
        Action<IConfigurationBuilder>? configureCallback = null)
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

        // Allow custom configuration via callback
        configureCallback?.Invoke(builder);

        return builder.Build();
    }

    /// <summary>
    /// Configure Serilog logging with optional customization callback
    /// </summary>
    private static void ConfigureSerilogLogging(
        IServiceCollection services, 
        Action<LoggerConfiguration>? configureCallback = null)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var logFileName = Path.Combine("logs", $"agent-run-{timestamp}.log");
        
        // Ensure logs directory exists
        Directory.CreateDirectory("logs");
        
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(logFileName,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                buffered: false,
                flushToDiskInterval: TimeSpan.FromSeconds(1));

        // Allow custom logging configuration via callback
        configureCallback?.Invoke(loggerConfig);
        
        Log.Logger = loggerConfig.CreateLogger();
        
        // Register logging with Serilog
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddSerilog(dispose: true);
        });
    }

    /// <summary>
    /// Register configuration models using Options pattern
    /// </summary>
    private static void RegisterConfigurationOptions(
        IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<AppConfiguration>(configuration);
        services.Configure<AgentFactoryConfiguration>(configuration.GetSection("agentFactory"));
        services.Configure<ProvidersConfiguration>(configuration.GetSection("providers"));
        services.Configure<ToolsConfiguration>(configuration.GetSection("tools"));
    }

    /// <summary>
    /// Register provider handlers using the provider package extension methods
    /// </summary>
    private static void RegisterProviderHandlers(IServiceCollection services)
    {
        // Use provider package extension methods to register providers
        // These will be automatically configured from appsettings.json via IConfiguration
        var sp = services.BuildServiceProvider();
        var configuration = sp.GetRequiredService<IConfiguration>();
        
        services.AddAzureOpenAIProvider(configuration.GetSection("providers:azureOpenAI"));
        services.AddOpenAIProvider(configuration.GetSection("providers:openAI"));
        services.AddGitHubModelsProvider(configuration.GetSection("providers:githubModels"));
    }

    /// <summary>
    /// Register tool providers and tool classes
    /// </summary>
    private static void RegisterToolProviders(IServiceCollection services)
    {
        services.AddSingleton<IToolProvider, LocalToolProvider>();
        services.AddSingleton<IToolProvider, McpToolProvider>();

        // Register tool classes (for dependency injection into LocalToolProvider)
        services.AddSingleton<WeatherTools>();
        // GitHubMockTools removed - GitHub tools will come from MCP server
    }

    /// <summary>
    /// Register core factory services
    /// </summary>
    private static void RegisterFactories(IServiceCollection services)
    {
        // Register core factories
        services.AddSingleton<IMarkdownAgentFactory, CoreMarkdownFactory>();
        services.AddSingleton<ProviderFactory>();
        services.AddSingleton<ToolFactory>();
        services.AddSingleton<AgentFramework.Factory.Services.AgentFactory>();
        
        // Register TestConsole wrappers for configuration-specific methods
        services.AddSingleton<TestConsole.Services.MarkdownAgentFactory>();
        services.AddSingleton<TestConsole.Services.AgentFactory>();
    }
}
