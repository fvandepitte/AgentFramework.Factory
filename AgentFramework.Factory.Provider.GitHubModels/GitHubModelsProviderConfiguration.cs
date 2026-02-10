using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.GitHubModels.Configuration;
using AgentFramework.Factory.Provider.GitHubModels.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Provider.GitHubModels;

/// <summary>
/// Provider configuration for GitHub Models
/// Implements IProviderConfiguration for provider extensibility
/// </summary>
public class GitHubModelsProviderConfiguration : IProviderConfiguration
{
    public string ProviderName => "GitHubModels";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddGitHubModelsProvider(configuration);
    }
}
