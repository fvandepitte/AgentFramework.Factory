using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.AzureOpenAI.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Provider.AzureOpenAI.Extensions;

/// <summary>
/// Extension methods for registering Azure OpenAI provider
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure OpenAI provider to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration containing AzureOpenAI settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureOpenAIProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureOpenAIConfiguration>(
            configuration.GetSection("AzureOpenAI"));

        services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();

        return services;
    }

    /// <summary>
    /// Adds Azure OpenAI provider to the service collection with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure Azure OpenAI options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureOpenAIProvider(
        this IServiceCollection services,
        Action<AzureOpenAIConfiguration> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IProviderHandler, AzureOpenAIProviderHandler>();

        return services;
    }
}
