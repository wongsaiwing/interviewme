namespace InterviewMe.Application.Abstractions;

public interface IKnowledgeIngestor
{
    Task IngestAsync(CancellationToken cancellationToken = default);
}
