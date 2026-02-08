using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Attributes;
using AgentFramework.Factory.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Reflection;

namespace AgentFramework.Factory.Services;

/// <summary>
/// Provides tools from local C# methods using dependency injection and LocalToolAttribute
/// </summary>
public class LocalToolProvider : IToolProvider
{
    private readonly IServiceProvider serviceProvider;
    private readonly ToolsConfiguration configuration;
    private readonly ILogger<LocalToolProvider> logger;
    private readonly Dictionary<string, AITool> discoveredTools = new();

    public LocalToolProvider(
        IServiceProvider serviceProvider,
        IOptions<ToolsConfiguration> configOptions,
        ILogger<LocalToolProvider> logger)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(logger);
        
        this.configuration = configOptions.Value;
        this.logger = logger;
        
        if (configuration.EnableLocalTools)
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
    /// Discovers tools from configured assemblies
    /// </summary>
    private void DiscoverTools()
    {
        var assemblies = new List<Assembly>();
        
        // If no assemblies configured, scan entry assembly
        if (configuration.ToolAssemblies == null || !configuration.ToolAssemblies.Any())
        {
            assemblies.Add(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
        }
        else
        {
            // Load configured assemblies
            foreach (var assemblyName in configuration.ToolAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to load tool assembly {AssemblyName}", assemblyName);
                }
            }
        }

        // Scan assemblies for tools
        foreach (var assembly in assemblies)
        {
            DiscoverToolsFromAssembly(assembly);
        }
    }

    /// <summary>
    /// Discovers tools from a specific assembly using LocalToolAttribute
    /// </summary>
    private void DiscoverToolsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            // Check if the class itself has LocalToolAttribute
            var classAttribute = type.GetCustomAttribute<LocalToolAttribute>();
            
            // Get methods with Description or LocalTool attribute
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null || 
                           m.GetCustomAttribute<LocalToolAttribute>() != null ||
                           classAttribute != null);

            foreach (var method in methods)
            {
                try
                {
                    var methodAttribute = method.GetCustomAttribute<LocalToolAttribute>();
                    var toolName = methodAttribute?.Name ?? method.Name;
                    
                    // Get description from attribute or Description attribute
                    var description = methodAttribute?.Description ??
                                    method.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                                    string.Empty;
                    
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
                            logger.LogDebug("Skipping tool {ToolName} from {TypeName}.{MethodName} - no instance available in DI", 
                                toolName, type.Name, method.Name);
                            continue;
                        }
                    }

                    // AIFunction implements AITool
                    discoveredTools[toolName] = aiFunction;
                    
                    logger.LogInformation("Discovered local tool: {ToolName} from {TypeName}.{MethodName}", 
                        toolName, type.Name, method.Name);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create tool from {TypeName}.{MethodName}", 
                        type.Name, method.Name);
                }
            }
        }
    }
}
