using InterviewMe.Application.Abstractions;
using Microsoft.Extensions.AI;

namespace InterviewMe.Infrastructure.Embeddings;

public sealed class HashingEmbeddingModel : IEmbeddingGenerator<string, Embedding<float>>, IEmbeddingClient
{
    private readonly HashingEmbeddingClient _inner;

    public HashingEmbeddingModel() : this(new HashingEmbeddingClient())
    {
    }

    public HashingEmbeddingModel(HashingEmbeddingClient inner)
    {
        _inner = inner;
    }

    public string Id => "hashing-local";

    public int Dimensions => HashingEmbeddingClient.Dimensions;

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        => _inner.EmbedAsync(text, cancellationToken);

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var list = new List<Embedding<float>>();
        foreach (var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vector = await _inner.EmbedAsync(value, cancellationToken).ConfigureAwait(false);
            list.Add(new Embedding<float>(vector));
        }

        return new GeneratedEmbeddings<Embedding<float>>(list);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose()
    {
    }
}
