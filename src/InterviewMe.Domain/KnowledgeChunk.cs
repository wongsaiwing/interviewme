namespace InterviewMe.Domain;

public sealed record KnowledgeChunk(
    string Id,
    string Source,
    string Title,
    string Text);
