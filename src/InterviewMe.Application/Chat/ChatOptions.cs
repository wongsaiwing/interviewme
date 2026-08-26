namespace InterviewMe.Application.Chat;

public sealed class ChatOptions
{
    public const string SectionName = "Chat";

    public int TopK { get; set; } = 4;
    public float MinScore { get; set; } = 0.08f;
    public int MaxMessageLength { get; set; } = 500;
    public int ConversationTurns { get; set; } = 3;
    public int RateLimitPerMinute { get; set; } = 30;
}
