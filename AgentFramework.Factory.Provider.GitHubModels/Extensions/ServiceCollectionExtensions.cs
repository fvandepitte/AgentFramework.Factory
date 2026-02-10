using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.GitHubModels.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Provider.GitHubModels.Extensions;

/// <summary>
/// Extension methods for registering GitHub Models provider
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds GitHub Models provider to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration containing GitHubModels settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGitHubModelsProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GitHubModelsConfiguration>(
            configuration.GetSection("GitHubModels"));

        services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();

        return services;
    }

    /// <summary>
    /// Adds GitHub Models provider to the service collection with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure GitHub Models options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGitHubModelsProvider(
        this IServiceCollection services,
        Action<GitHubModelsConfiguration> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IProviderHandler, GitHubModelsProviderHandler>();

        return services;
    }
}
