using System.Collections.Concurrent;
using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using Microsoft.Extensions.Options;

namespace InterviewMe.Infrastructure.RateLimiting;

public sealed class InMemoryRateLimiter : IChatRateLimiter
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _hits = new();
    private readonly ChatOptions _options;

    public InMemoryRateLimiter(IOptions<ChatOptions> options)
    {
        _options = options.Value;
    }

    public bool TryAcquire(string clientKey)
    {
        var key = string.IsNullOrWhiteSpace(clientKey) ? "anonymous" : clientKey;
        var window = _hits.GetOrAdd(key, _ => new ConcurrentQueue<long>());
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var cutoff = now - 60_000;

        while (window.TryPeek(out var oldest) && oldest < cutoff)
        {
            window.TryDequeue(out _);
        }

        if (window.Count >= Math.Max(1, _options.RateLimitPerMinute))
        {
            return false;
        }

        window.Enqueue(now);
        return true;
    }
}
