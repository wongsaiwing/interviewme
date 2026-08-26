using System.Runtime.CompilerServices;
using InterviewMe.Application.Abstractions;
using InterviewMe.Domain;
using Microsoft.Extensions.Logging;

namespace InterviewMe.Infrastructure.Llm;

public sealed class FallbackLlmClient : ILlmClient
{
    private readonly OpenAiCompatibleLlmClient? _primary;
    private readonly StubLlmClient _stub;
    private readonly ILogger<FallbackLlmClient> _logger;

    public FallbackLlmClient(
        StubLlmClient stub,
        ILogger<FallbackLlmClient> logger,
        OpenAiCompatibleLlmClient? primary = null)
    {
        _stub = stub;
        _logger = logger;
        _primary = primary;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        ChatPrompt prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_primary is not null)
        {
            var received = false;
            var failed = false;
            await using var enumerator = _primary
                .StreamCompletionAsync(prompt, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);

            while (true)
            {
                var moved = false;
                try
                {
                    moved = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OpenAI-compatible stream failed");
                    failed = true;
                }

                if (failed || !moved)
                {
                    break;
                }

                received = true;
                yield return enumerator.Current;
            }

            if (received)
            {
                yield break;
            }

            _logger.LogWarning("OpenAI-compatible client returned nothing; using grounded stub.");
        }

        await foreach (var token in _stub.StreamCompletionAsync(prompt, cancellationToken))
        {
            yield return token;
        }
    }
}
