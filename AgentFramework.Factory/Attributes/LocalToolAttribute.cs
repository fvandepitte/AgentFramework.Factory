namespace AgentFramework.Factory.Attributes;

/// <summary>
/// Marks a class or method as a local tool that should be discovered and registered by the LocalToolProvider.
/// Apply this attribute to classes containing tool methods or directly to individual tool methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class LocalToolAttribute : Attribute
{
    /// <summary>
    /// Optional name for the tool. If not specified, the class/method name will be used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional description for the tool. If not specified, will try to use Description attribute.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Creates a new LocalToolAttribute with optional name and description
    /// </summary>
    public LocalToolAttribute()
    {
    }

    /// <summary>
    /// Creates a new LocalToolAttribute with a specific name
    /// </summary>
    /// <param name="name">The name of the tool</param>
    public LocalToolAttribute(string name)
    {
        Name = name;
    }
}
