using System.Text.Json;
using InterviewMe.Application.Abstractions;
using InterviewMe.Domain;
using InterviewMe.Infrastructure.Embeddings;
using LangChain.Schema;
using Microsoft.Extensions.VectorData;
using SkInMemory = Microsoft.SemanticKernel.Connectors.InMemory;

namespace InterviewMe.Infrastructure.VectorStore;

/// <summary>
/// Application <see cref="IVectorStore"/> over a LangChain in-memory
/// <see cref="VectorStoreCollection{TKey,TRecord}"/> of <see cref="LangChainDocumentRecord"/>.
/// Similarity search is LangChain/MEVA <c>SearchAsync</c>; hashing embeddings keep a lexical overlay
/// so a tiny CV corpus does not return unrelated biography.
/// </summary>
public sealed class LangChainVectorStore : IVectorStore
{
    public const string CollectionName = "interviewme-facts";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly SkInMemory.InMemoryVectorStore _store;
    private readonly VectorStoreCollection<string, LangChainDocumentRecord> _collection;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly List<RetrievedFact> _allFacts = new();
    private int _count;

    public LangChainVectorStore()
    {
        _store = new SkInMemory.InMemoryVectorStore(new SkInMemory.InMemoryVectorStoreOptions());
        var definition = new VectorStoreCollectionDefinition
        {
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(string)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreDataProperty("MetadataJson", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(ReadOnlyMemory<float>), HashingEmbeddingClient.Dimensions)
            ]
        };
        _collection = _store.GetCollection<string, LangChainDocumentRecord>(CollectionName, definition);
    }

    public VectorStoreCollection<string, LangChainDocumentRecord> Collection => _collection;

    public async Task UpsertAsync(
        IReadOnlyList<(KnowledgeChunk Chunk, float[] Embedding)> items,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _collection.EnsureCollectionDeletedAsync(cancellationToken).ConfigureAwait(false);
            await _collection.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);

            var records = items.Select(item => new LangChainDocumentRecord
            {
                Id = item.Chunk.Id,
                Text = item.Chunk.Text,
                MetadataJson = JsonSerializer.Serialize(new FactMeta(item.Chunk.Source, item.Chunk.Title), JsonOptions),
                Embedding = item.Embedding
            }).ToList();

            if (records.Count > 0)
            {
                await _collection.UpsertAsync(records, cancellationToken).ConfigureAwait(false);
            }

            _allFacts.Clear();
            foreach (var item in items)
            {
                _allFacts.Add(new RetrievedFact(
                    item.Chunk.Id,
                    item.Chunk.Source,
                    item.Chunk.Title,
                    item.Chunk.Text,
                    1.0f));
            }

            _count = records.Count;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task<IReadOnlyList<RetrievedFact>> GetBySourcePrefixAsync(
        string sourcePrefix,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<RetrievedFact> hits = _allFacts
            .Where(f => f.Source.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(hits);
    }

    public async Task<IReadOnlyList<RetrievedFact>> SearchAsync(
        float[] queryEmbedding,
        string queryText,
        int topK,
        CancellationToken cancellationToken = default)
    {
        await _collection.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);

        var take = Math.Max(Math.Max(1, topK) * 5, Math.Max(_count, 8));
        var queryTokens = new HashSet<string>(HashingEmbeddingClient.Tokenize(queryText));
        var ranked = new List<RetrievedFact>();

        await foreach (var hit in _collection.SearchAsync(queryEmbedding, take, cancellationToken: cancellationToken)
                           .ConfigureAwait(false))
        {
            var record = hit.Record;
            var meta = ParseMeta(record.MetadataJson);
            var text = record.Text ?? string.Empty;
            var cosine = hit.Score is { } s ? Clamp01((float)s) : Cosine(queryEmbedding, record.Embedding.ToArray());
            var overlap = LexicalOverlap(queryTokens, (meta.Title + " " + text));
            if (overlap <= 0)
            {
                continue;
            }

            var score = (0.65f * cosine) + (0.35f * overlap);
            ranked.Add(new RetrievedFact(
                record.Id,
                meta.Source,
                meta.Title,
                text,
                score));
        }

        return ranked
            .OrderByDescending(f => f.Score)
            .Take(Math.Max(1, topK))
            .ToList();
    }

    internal static float Cosine(float[] a, float[] b)
    {
        var n = Math.Min(a.Length, b.Length);
        double dot = 0, na = 0, nb = 0;
        for (var i = 0; i < n; i++)
        {
            dot += a[i] * (double)b[i];
            na += a[i] * (double)a[i];
            nb += b[i] * (double)b[i];
        }

        var denom = Math.Sqrt(na) * Math.Sqrt(nb);
        if (denom < 1e-8)
        {
            return 0;
        }

        return (float)(dot / denom);
    }

    internal static float LexicalOverlap(HashSet<string> queryTokens, string document)
    {
        if (queryTokens.Count == 0)
        {
            return 0;
        }

        var docTokens = HashingEmbeddingClient.Tokenize(document);
        if (docTokens.Count == 0)
        {
            return 0;
        }

        var docSet = new HashSet<string>(docTokens);
        var hits = queryTokens.Count(docSet.Contains);
        return (float)hits / queryTokens.Count;
    }

    private static float Clamp01(float value)
    {
        if (value < 0)
        {
            return 0;
        }

        return value > 1 ? 1 : value;
    }

    private static FactMeta ParseMeta(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new FactMeta("unknown", "untitled");
        }

        try
        {
            return JsonSerializer.Deserialize<FactMeta>(json, JsonOptions) ?? new FactMeta("unknown", "untitled");
        }
        catch (JsonException)
        {
            return new FactMeta("unknown", "untitled");
        }
    }

    private sealed record FactMeta(string Source, string Title);
}
