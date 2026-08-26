namespace InterviewMe.Domain;

public sealed record RetrievedFact(
    string Id,
    string Source,
    string Title,
    string Text,
    float Score);
