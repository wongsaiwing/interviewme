using InterviewMe.Application.Abstractions;

namespace InterviewMe.Application.Chat;

public sealed class DefaultToneLibrary : IToneLibrary
{
    public string GetFewShots() => PromptBuilder.DefaultTone;
}
