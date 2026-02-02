using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Interface for provider configuration extensibility
/// Allows providers to self-register and configure their dependencies
/// </summary>
public interface IProviderConfiguration
{
    /// <summary>
    /// Gets the name of the provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Configures the provider's services in the dependency injection container
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The configuration to use</param>
    void Configure(IServiceCollection services, IConfiguration configuration);
}
