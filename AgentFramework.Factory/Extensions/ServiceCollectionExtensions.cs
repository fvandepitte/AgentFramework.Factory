using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Configuration;
using AgentFramework.Factory.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Extensions;

/// <summary>
/// Service collection extensions for AgentFramework.Factory
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Agent Framework Factory services to the service collection
    /// </summary>
    public static IAgentFrameworkBuilder AddAgentFramework(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register configuration if provided
        if (configuration != null)
        {
            services.Configure<AgentFactoryConfiguration>(
                configuration.GetSection("agentFactory"));
            services.Configure<ToolsConfiguration>(
                configuration.GetSection("tools"));
        }

        // Register core factory
        services.AddSingleton<IMarkdownAgentFactory, MarkdownAgentFactory>();

        return new AgentFrameworkBuilder(services);
    }

    /// <summary>
    /// Adds the Agent Framework Factory services with configuration callback
    /// </summary>
    public static IAgentFrameworkBuilder AddAgentFramework(
        this IServiceCollection services,
        Action<AgentFactoryConfiguration> configureFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureFactory);

        services.Configure(configureFactory);
        services.AddSingleton<IMarkdownAgentFactory, MarkdownAgentFactory>();

        return new AgentFrameworkBuilder(services);
    }
}

/// <summary>
/// Builder interface for configuring Agent Framework
/// </summary>
public interface IAgentFrameworkBuilder
{
    /// <summary>
    /// Gets the service collection
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds markdown agent support with configuration
    /// </summary>
    IAgentFrameworkBuilder AddMarkdownAgents(Action<AgentFactoryConfiguration> configure);

    /// <summary>
    /// Adds a provider handler
    /// </summary>
    IAgentFrameworkBuilder AddProvider<TProvider>() where TProvider : class, IProviderHandler;

    /// <summary>
    /// Adds a provider handler with configuration
    /// </summary>
    IAgentFrameworkBuilder AddProvider<TProvider>(Action<IServiceCollection> configure) 
        where TProvider : class, IProviderHandler;

    /// <summary>
    /// Adds a tool provider
    /// </summary>
    IAgentFrameworkBuilder AddToolProvider<TToolProvider>() where TToolProvider : class, IToolProvider;
}

/// <summary>
/// Default implementation of IAgentFrameworkBuilder
/// </summary>
internal class AgentFrameworkBuilder : IAgentFrameworkBuilder
{
    public IServiceCollection Services { get; }

    public AgentFrameworkBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IAgentFrameworkBuilder AddMarkdownAgents(Action<AgentFactoryConfiguration> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public IAgentFrameworkBuilder AddProvider<TProvider>() where TProvider : class, IProviderHandler
    {
        Services.AddSingleton<IProviderHandler, TProvider>();
        return this;
    }

    public IAgentFrameworkBuilder AddProvider<TProvider>(Action<IServiceCollection> configure) 
        where TProvider : class, IProviderHandler
    {
        configure(Services);
        Services.AddSingleton<IProviderHandler, TProvider>();
        return this;
    }

    public IAgentFrameworkBuilder AddToolProvider<TToolProvider>() 
        where TToolProvider : class, IToolProvider
    {
        Services.AddSingleton<IToolProvider, TToolProvider>();
        return this;
    }
}
