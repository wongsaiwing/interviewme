using InterviewMe.Domain;

namespace InterviewMe.Application.Abstractions;

public interface ILlmClient
{
    IAsyncEnumerable<string> StreamCompletionAsync(
        ChatPrompt prompt,
        CancellationToken cancellationToken = default);
}
