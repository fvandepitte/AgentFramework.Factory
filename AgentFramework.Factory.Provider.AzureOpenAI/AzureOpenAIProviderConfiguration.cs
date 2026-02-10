using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Provider.AzureOpenAI.Configuration;
using AgentFramework.Factory.Provider.AzureOpenAI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Factory.Provider.AzureOpenAI;

/// <summary>
/// Provider configuration for Azure OpenAI
/// Implements IProviderConfiguration for provider extensibility
/// </summary>
public class AzureOpenAIProviderConfiguration : IProviderConfiguration
{
    public string ProviderName => "AzureOpenAI";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureOpenAIProvider(configuration);
    }
}
