namespace InterviewMe.Domain;

public sealed record Profile(
    string Name,
    string Title,
    string ShortBio,
    string AvatarInitials,
    IReadOnlyList<string> SuggestedQuestions);
