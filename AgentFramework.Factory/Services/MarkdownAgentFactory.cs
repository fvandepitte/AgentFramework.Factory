using AgentFramework.Factory.Abstractions;
using AgentFramework.Factory.Configuration;
using AgentFramework.Factory.Exceptions;
using AgentFramework.Factory.Models;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;

namespace AgentFramework.Factory.Services;

/// <summary>
/// Factory class for creating agents from markdown files
/// </summary>
public class MarkdownAgentFactory : IMarkdownAgentFactory
{
    private readonly AgentFactoryConfiguration configuration;
    private readonly ILogger<MarkdownAgentFactory> logger;
    private readonly MarkdownPipeline markdownPipeline;
    private readonly IDeserializer yamlDeserializer;

    public MarkdownAgentFactory(
        IOptions<AgentFactoryConfiguration> configOptions,
        ILogger<MarkdownAgentFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ArgumentNullException.ThrowIfNull(logger);
        
        this.configuration = configOptions.Value;
        this.logger = logger;

        // Setup Markdig pipeline with YAML frontmatter support
        markdownPipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        // Setup YAML deserializer
        yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public ILoadedAgent LoadAgentFromFile(string markdownPath, string? provider = null)
    {
        try
        {
            if (!File.Exists(markdownPath))
            {
                throw new AgentLoadException($"Markdown file not found: {markdownPath}")
                {
                    FilePath = markdownPath
                };
            }

            var markdownContent = File.ReadAllText(markdownPath);
            return ParseMarkdown(markdownContent, provider);
        }
        catch (Exception ex) when (ex is not AgentLoadException)
        {
            throw new AgentLoadException($"Failed to load agent from file: {markdownPath}", ex)
            {
                FilePath = markdownPath
            };
        }
    }

    public ILoadedAgent ParseMarkdown(string markdownContent, string? provider = null)
    {
        try
        {
            // Parse markdown document
            var document = Markdown.Parse(markdownContent, markdownPipeline);

            // Extract YAML frontmatter
            var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
            if (yamlBlock == null)
            {
                throw new AgentLoadException("No YAML frontmatter found in markdown");
            }

            // Deserialize YAML to metadata
            var yamlText = string.Join("\n", yamlBlock.Lines.Lines.Select(l => l.ToString()));
            var metadata = yamlDeserializer.Deserialize<AgentMetadata>(yamlText);

            if (string.IsNullOrWhiteSpace(metadata.Name))
            {
                throw new AgentLoadException("Agent name is required in YAML frontmatter");
            }

            // Extract markdown body as instructions (skip YAML frontmatter)
            var instructions = ExtractMarkdownBody(markdownContent);

            // Build the loaded agent
            return new LoadedAgent
            {
                Name = metadata.Name,
                Description = metadata.Description,
                Model = metadata.Model,
                Instructions = instructions,
                Tools = metadata.Tools.AsReadOnly(),
                Temperature = metadata.Temperature ?? 0.7,
                MaxTokens = metadata.MaxTokens,
                TopP = metadata.TopP,
                FrequencyPenalty = metadata.FrequencyPenalty,
                PresencePenalty = metadata.PresencePenalty,
                Provider = provider ?? metadata.Provider
            };
        }
        catch (Exception ex) when (ex is not AgentLoadException)
        {
            throw new AgentLoadException("Failed to parse markdown content", ex);
        }
    }

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
