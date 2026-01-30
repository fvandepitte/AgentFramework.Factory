using AgentFramework.Factory.TestConsole.Services.Configuration;
using AgentFramework.Factory.TestConsole.Services.Models;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;

namespace AgentFramework.Factory.TestConsole.Services.Factories;

/// <summary>
/// Factory class for creating agents from markdown files
/// </summary>
public class MarkdownAgentFactory
{
    private readonly AppConfiguration configuration;
    private readonly MarkdownPipeline markdownPipeline;
    private readonly IDeserializer yamlDeserializer;

    public MarkdownAgentFactory(IOptions<AppConfiguration> configOptions)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        this.configuration = configOptions.Value;
        
        // Setup Markdig pipeline with YAML frontmatter support
        markdownPipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();
        
        // Setup YAML deserializer
        yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Load all agents defined in the configuration
    /// </summary>
    public List<LoadedAgent> LoadAgentsFromConfiguration()
    {
        var agents = new List<LoadedAgent>();

        foreach (var agentConfig in configuration.Agents.Where(a => a.Enabled))
        {
            try
            {
                var agent = LoadAgentFromMarkdown(agentConfig);
                agents.Add(agent);
                
                if (configuration.AgentFactory.EnableLogging)
                {
                    Console.WriteLine($"  ✓ Loaded: {agent.Name} from {agentConfig.MarkdownPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Failed to load {agentConfig.Name}: {ex.Message}");
            }
        }

        return agents;
    }

    /// <summary>
    /// Load an agent from a markdown file
    /// </summary>
    public LoadedAgent LoadAgentFromMarkdown(AgentConfigurationEntry config)
    {
        if (!File.Exists(config.MarkdownPath))
        {
            throw new FileNotFoundException($"Agent markdown file not found: {config.MarkdownPath}");
        }

        var markdownContent = File.ReadAllText(config.MarkdownPath);
        
        // Parse the markdown document
        var document = Markdown.Parse(markdownContent, markdownPipeline);
        
        // Extract YAML frontmatter
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        
        AgentMetadata? metadata = null;
        if (yamlBlock != null)
        {
            var yamlText = string.Join("\n", yamlBlock.Lines.Lines.Select(l => l.ToString()));
            metadata = yamlDeserializer.Deserialize<AgentMetadata>(yamlText);
        }

        if (metadata == null)
        {
            throw new InvalidOperationException($"No YAML frontmatter found in {config.MarkdownPath}");
        }

        // Extract markdown body as plain text (instructions)
        var instructions = ExtractMarkdownBody(markdownContent);

        // Create loaded agent with configuration overrides applied
        var agent = new LoadedAgent
        {
            Name = config.Name ?? metadata.Name,
            Description = metadata.Description,
            Model = config.Overrides?.Model ?? metadata.Model,
            Temperature = config.Overrides?.Temperature ?? metadata.Temperature,
            MaxTokens = config.Overrides?.MaxTokens ?? metadata.MaxTokens,
            TopP = config.Overrides?.TopP ?? metadata.TopP,
            FrequencyPenalty = config.Overrides?.FrequencyPenalty ?? metadata.FrequencyPenalty,
            PresencePenalty = config.Overrides?.PresencePenalty ?? metadata.PresencePenalty,
            Instructions = instructions,
            SourceFile = config.MarkdownPath,
            Provider = config.Provider ?? configuration.AgentFactory.DefaultProvider,
            Tools = metadata.Tools ?? new List<string>()
        };

        return agent;
    }

    /// <summary>
    /// Load agent directly from a markdown file path
    /// </summary>
    public LoadedAgent LoadAgentFromFile(string markdownPath, string? provider = null)
    {
        var config = new AgentConfigurationEntry
        {
            Name = Path.GetFileNameWithoutExtension(markdownPath),
            MarkdownPath = markdownPath,
            Provider = provider
        };

        return LoadAgentFromMarkdown(config);
    }

    /// <summary>
    /// Discover all markdown files in the configured directory
    /// </summary>
    public List<string> DiscoverAgentFiles()
    {
        var path = configuration.AgentFactory.AgentDefinitionsPath;
        var pattern = configuration.AgentFactory.AgentFilePattern;

        if (!Directory.Exists(path))
        {
            return new List<string>();
        }

        return Directory.GetFiles(path, pattern, SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    /// Extract the markdown body (everything after YAML frontmatter)
    /// </summary>
    private string ExtractMarkdownBody(string markdownContent)
    {
        // Remove YAML frontmatter block (between --- delimiters)
        var lines = markdownContent.Split('\n');
        var inFrontmatter = false;
        var frontmatterCount = 0;
        var bodyLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.Trim() == "---")
            {
                frontmatterCount++;
                if (frontmatterCount == 1)
                {
                    inFrontmatter = true;
                }
                else if (frontmatterCount == 2)
                {
                    inFrontmatter = false;
                    continue; // Skip the closing ---
                }
            }
            else if (!inFrontmatter && frontmatterCount >= 2)
            {
                bodyLines.Add(line);
            }
        }

        return string.Join("\n", bodyLines).Trim();
    }
}
