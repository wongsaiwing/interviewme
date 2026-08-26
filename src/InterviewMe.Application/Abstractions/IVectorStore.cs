using InterviewMe.Domain;

namespace InterviewMe.Application.Abstractions;

public interface IVectorStore
{
    Task UpsertAsync(
        IReadOnlyList<(KnowledgeChunk Chunk, float[] Embedding)> items,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RetrievedFact>> SearchAsync(
        float[] queryEmbedding,
        string queryText,
        int topK,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RetrievedFact>> GetBySourcePrefixAsync(
        string sourcePrefix,
        CancellationToken cancellationToken = default);
}
