using System.Text.Json.Serialization;

namespace InterviewMe.Application.Chat;

public sealed record ChatCommand(
    string Message,
    string SessionId,
    string ClientKey);

public sealed record SourceCitation(string Id, string Source, string Title, float Score);

public sealed record ChatStreamEvent(
    string Type,
    string? Text = null,
    [property: JsonIgnore] IReadOnlyList<SourceCitation>? Sources = null,
    string? Error = null)
{
    public static ChatStreamEvent Token(string text) => new("token", Text: text);

    public static ChatStreamEvent Status(string stage) => new("status", Text: stage);

    public static ChatStreamEvent WithSources(IReadOnlyList<SourceCitation> sources) =>
        new("sources", Sources: sources);

    public static ChatStreamEvent Done() => new("done");

    public static ChatStreamEvent Failure(string message) => new("error", Error: message);
}
