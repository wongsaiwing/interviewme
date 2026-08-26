using System.Collections.Concurrent;
using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using Microsoft.Extensions.Options;

namespace InterviewMe.Infrastructure.RateLimiting;

public sealed class InMemoryRateLimiter : IChatRateLimiter
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _minute = new();
    private readonly ConcurrentDictionary<string, DayBucket> _day = new();
    private readonly ChatOptions _options;

    public InMemoryRateLimiter(IOptions<ChatOptions> options)
    {
        _options = options.Value;
    }

    public bool TryAcquire(string clientKey)
    {
        var key = string.IsNullOrWhiteSpace(clientKey) ? "anonymous" : clientKey.Trim();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var minute = _minute.GetOrAdd(key, _ => new ConcurrentQueue<long>());
        var cutoff = now - 60_000;
        while (minute.TryPeek(out var oldest) && oldest < cutoff)
        {
            minute.TryDequeue(out _);
        }

        if (minute.Count >= Math.Max(1, _options.RateLimitPerMinute))
        {
            return false;
        }

        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var bucket = _day.GetOrAdd(key, _ => new DayBucket { Day = today });
        lock (bucket)
        {
            if (bucket.Day != today)
            {
                bucket.Day = today;
                bucket.Count = 0;
            }

            var dailyCap = Math.Max(1, _options.DailyRequestLimit);
            if (bucket.Count >= dailyCap)
            {
                return false;
            }

            bucket.Count++;
        }

        minute.Enqueue(now);
        return true;
    }

    private sealed class DayBucket
    {
        public string Day { get; set; } = "";
        public int Count { get; set; }
    }
}
