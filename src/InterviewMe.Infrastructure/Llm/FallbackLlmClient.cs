using System.Net;
using System.Runtime.CompilerServices;
using InterviewMe.Application.Abstractions;
using InterviewMe.Domain;
using Microsoft.Extensions.Logging;

namespace InterviewMe.Infrastructure.Llm;

/// <summary>
/// Wraps the LLM provider. On any provider failure or empty reply it throws
/// <see cref="LlmUnavailableException"/>; it never streams retrieved chunks, prompt text,
/// or instruction text. The grounded stub is used only when explicitly allowed
/// (no API key in Development, or Llm:AllowStub=true) and never as a failure fallback.
/// </summary>
public sealed class FallbackLlmClient : ILlmClient
{
    private readonly ILlmClient? _primary;
    private readonly StubLlmClient? _stub;
    private readonly ILogger<FallbackLlmClient> _logger;

    public FallbackLlmClient(
        StubLlmClient? stub,
        ILogger<FallbackLlmClient> logger,
        ILlmClient? primary = null)
    {
        _stub = stub;
        _logger = logger;
        _primary = primary;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        ChatPrompt prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_primary is null)
        {
            if (_stub is null)
            {
                _logger.LogWarning("No LLM provider configured and stub not allowed.");
                throw new LlmUnavailableException(rateLimited: false);
            }

            await foreach (var token in _stub.StreamCompletionAsync(prompt, cancellationToken))
            {
                yield return token;
            }

            yield break;
        }

        var received = false;
        await using var enumerator = _primary
            .StreamCompletionAsync(prompt, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            bool moved;
            try
            {
                moved = await enumerator.MoveNextAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var rateLimited = ex is HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests };
                _logger.LogWarning(ex, "LLM provider stream failed (rateLimited={RateLimited})", rateLimited);
                throw new LlmUnavailableException(rateLimited, ex);
            }

            if (!moved)
            {
                break;
            }

            if (!string.IsNullOrEmpty(enumerator.Current))
            {
                received = true;
            }

            yield return enumerator.Current;
        }

        if (!received)
        {
            _logger.LogWarning("LLM provider returned an empty reply.");
            throw new LlmUnavailableException(rateLimited: false);
        }
    }
}
