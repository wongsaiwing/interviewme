using System.Collections.Concurrent;
using InterviewMe.Application.Abstractions;
using InterviewMe.Domain;

namespace InterviewMe.Infrastructure.Conversation;

/// <summary>
/// Short-lived session memory only. Visitor transcripts are never written to disk.
/// </summary>
public sealed class InMemoryConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public IReadOnlyList<ChatMessage> GetRecent(string sessionId, int take)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return [];
        }

        if (DateTimeOffset.UtcNow - session.TouchedUtc > Ttl)
        {
            _sessions.TryRemove(sessionId, out _);
            return [];
        }

        lock (session.Gate)
        {
            if (session.Messages.Count <= take)
            {
                return session.Messages.ToArray();
            }

            return session.Messages.Skip(session.Messages.Count - take).ToArray();
        }
    }

    public void Append(string sessionId, ChatMessage message)
    {
        var session = _sessions.GetOrAdd(sessionId, _ => new Session());
        lock (session.Gate)
        {
            session.Messages.Add(message);
            session.TouchedUtc = DateTimeOffset.UtcNow;
            const int cap = 24;
            if (session.Messages.Count > cap)
            {
                session.Messages.RemoveRange(0, session.Messages.Count - cap);
            }
        }
    }

    private sealed class Session
    {
        public object Gate { get; } = new();
        public List<ChatMessage> Messages { get; } = [];
        public DateTimeOffset TouchedUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}
