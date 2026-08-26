namespace InterviewMe.Domain;

public sealed record LlmMessage(string Role, string Content);

public sealed record ChatPrompt(
    IReadOnlyList<LlmMessage> Messages,
    IReadOnlyList<RetrievedFact> Facts,
    bool HasGrounding);
