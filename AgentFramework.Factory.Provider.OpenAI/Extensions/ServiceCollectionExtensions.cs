using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.OpenAI.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Provider.OpenAI.Extensions;

/// <summary>
/// Extension methods for registering OpenAI provider
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAI provider to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration containing OpenAI settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOpenAIProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenAIConfiguration>(
            configuration.GetSection("OpenAI"));

        services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();

        return services;
    }

    /// <summary>
    /// Adds OpenAI provider to the service collection with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure OpenAI options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOpenAIProvider(
        this IServiceCollection services,
        Action<OpenAIConfiguration> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IProviderHandler, OpenAIProviderHandler>();

        return services;
    }
}
