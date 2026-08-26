using InterviewMe.Application.Abstractions;
using InterviewMe.Domain;
using Microsoft.Extensions.Logging;

namespace InterviewMe.Infrastructure.Knowledge;

public sealed class MarkdownKnowledgeIngestor : IKnowledgeIngestor
{
    private readonly IEmbeddingClient _embeddings;
    private readonly IVectorStore _store;
    private readonly ILogger<MarkdownKnowledgeIngestor> _logger;
    private readonly string _path;

    public MarkdownKnowledgeIngestor(
        IEmbeddingClient embeddings,
        IVectorStore store,
        ILogger<MarkdownKnowledgeIngestor> logger,
        string knowledgePath)
    {
        _embeddings = embeddings;
        _store = store;
        _logger = logger;
        _path = knowledgePath;
    }

    public async Task IngestAsync(CancellationToken cancellationToken = default)
    {
        var files = EnumerateFactFiles(_path).ToArray();
        if (files.Length == 0)
        {
            throw new InvalidOperationException($"No fact markdown files found under '{_path}' (expected knowledge/facts).");
        }

        var items = new List<(KnowledgeChunk Chunk, float[] Embedding)>();
        foreach (var file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var markdown = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            foreach (var chunk in LangChainMarkdownSplitter.SplitFile(file, markdown))
            {
                var embedding = await _embeddings.EmbedAsync($"{chunk.Title}\n{chunk.Text}", cancellationToken)
                    .ConfigureAwait(false);
                items.Add((chunk, embedding));
            }
        }

        await _store.UpsertAsync(items, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation(
            "Ingested {Count} knowledge chunks from {Path} via LangChainMarkdownSplitter (facts only; tone skipped)",
            items.Count,
            _path);
    }

    internal static IEnumerable<string> EnumerateFactFiles(string knowledgePath)
    {
        var factsDir = Path.Combine(knowledgePath, "facts");
        var root = Directory.Exists(factsDir) ? factsDir : knowledgePath;
        if (!Directory.Exists(root))
        {
            return [];
        }

        return Directory.GetFiles(root, "*.md", SearchOption.AllDirectories)
            .Where(IsFactFile);
    }

    internal static bool IsFactFile(string path)
    {
        var name = Path.GetFileName(path);
        if (string.Equals(name, "README.md", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var full = Path.GetFullPath(path);
        var parts = full.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return !parts.Any(p => string.Equals(p, "tone", StringComparison.OrdinalIgnoreCase));
    }
}
