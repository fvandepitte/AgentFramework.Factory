namespace AgentFramework.Factory.Abstractions;

/// <summary>
/// Defines a factory for loading agents from markdown files
/// </summary>
public interface IMarkdownAgentFactory
{
    /// <summary>
    /// Loads an agent from a markdown file
    /// </summary>
    /// <param name="markdownPath">Path to the markdown file</param>
    /// <param name="provider">Optional provider override</param>
    /// <returns>Loaded agent representation</returns>
    ILoadedAgent LoadAgentFromFile(string markdownPath, string? provider = null);

    /// <summary>
    /// Parses markdown content into an agent representation
    /// </summary>
    /// <param name="markdownContent">The markdown content</param>
    /// <param name="provider">Optional provider override</param>
    /// <returns>Loaded agent representation</returns>
    ILoadedAgent ParseMarkdown(string markdownContent, string? provider = null);
}
