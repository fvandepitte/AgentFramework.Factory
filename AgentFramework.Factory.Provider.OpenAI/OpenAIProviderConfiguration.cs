using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.OpenAI.Configuration;
using AgentFramework.Factory.Provider.OpenAI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Provider.OpenAI;

/// <summary>
/// Provider configuration for OpenAI
/// Implements IProviderConfiguration for provider extensibility
/// </summary>
public class OpenAIProviderConfiguration : IProviderConfiguration
{
    public string ProviderName => "OpenAI";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenAIProvider(configuration);
    }
}
