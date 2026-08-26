using InterviewMe.Domain;

namespace InterviewMe.Application.Abstractions;

public interface IConversationStore
{
    IReadOnlyList<ChatMessage> GetRecent(string sessionId, int take);
    void Append(string sessionId, ChatMessage message);
}
