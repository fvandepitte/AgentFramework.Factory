using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.TestConsole.Services.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Reflection;

namespace AgentFramework.Factory.TestConsole.Services.Tools;

/// <summary>
/// Provides tools from local C# methods using dependency injection
/// </summary>
public class LocalToolProvider : IToolProvider
{
    private readonly IServiceProvider serviceProvider;
    private readonly AppConfiguration configuration;
    private readonly Dictionary<string, AITool> discoveredTools = new();

    public LocalToolProvider(
        IServiceProvider serviceProvider,
        IOptions<AppConfiguration> configOptions)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(configOptions);
        this.configuration = configOptions.Value;
        
        if (configuration.AgentFactory.EnableToolDiscovery)
        {
            DiscoverTools();
        }
    }

    public string Name => "Local";
    public string Type => "local";

    public bool CanProvide(string toolName)
    {
        return discoveredTools.ContainsKey(toolName);
    }

    public IEnumerable<AITool> GetTools(IEnumerable<string> toolNames)
    {
        foreach (var toolName in toolNames)
        {
            if (discoveredTools.TryGetValue(toolName, out var tool))
            {
                yield return tool;
            }
        }
    }

    public IEnumerable<AITool> GetAllTools()
    {
        return discoveredTools.Values;
    }

    /// <summary>
    /// Discovers tools from registered assemblies
    /// </summary>
    private void DiscoverTools()
    {
        var assemblies = new List<Assembly> { Assembly.GetExecutingAssembly() };
        
        // Load additional assemblies from configuration
        foreach (var assemblyName in configuration.AgentFactory.ToolAssemblies)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                if (configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ⚠ Failed to load tool assembly {assemblyName}: {ex.Message}");
                }
            }
        }

        // Scan assemblies for tool methods
        foreach (var assembly in assemblies)
        {
            DiscoverToolsFromAssembly(assembly);
        }
    }

    /// <summary>
    /// Discovers tools from a specific assembly
    /// </summary>
    private void DiscoverToolsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);

            foreach (var method in methods)
            {
                try
                {
                    var toolName = method.Name;
                    var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                    
                    // Create AIFunction from the method
                    AIFunction aiFunction;
                    if (method.IsStatic)
                    {
                        aiFunction = AIFunctionFactory.Create(method, target: null, name: toolName, description: description);
                    }
                    else
                    {
                        // For instance methods, try to get instance from DI
                        var instance = serviceProvider.GetService(type);
                        if (instance != null)
                        {
                            aiFunction = AIFunctionFactory.Create(method, target: instance, name: toolName, description: description);
                        }
                        else
                        {
                            continue; // Skip if can't get instance
                        }
                    }

                    // AIFunction implements AITool, so we can store it directly
                    discoveredTools[toolName] = aiFunction;
                    
                    if (configuration.AgentFactory.EnableLogging)
                    {
                        Console.WriteLine($"  ✓ Discovered tool: {toolName} from {type.Name}.{method.Name}");
                    }
                }
                catch (Exception ex)
                {
                    if (configuration.AgentFactory.EnableLogging)
                    {
                        Console.WriteLine($"  ⚠ Failed to create tool from {type.Name}.{method.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}
