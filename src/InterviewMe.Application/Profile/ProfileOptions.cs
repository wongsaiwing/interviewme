namespace InterviewMe.Application.Profile;

public sealed class ProfileOptions
{
    public const string SectionName = "Profile";

    public string Name { get; set; } = "Silas Wong";
    public string Title { get; set; } = "Assistant Solution Analyst, HAECO";
    public string ShortBio { get; set; } =
        "Full-stack developer in Hong Kong. End-to-end ownership from requirements to production.";
    public string AvatarInitials { get; set; } = "SW";
    public List<string> SuggestedQuestions { get; set; } = [];
}

public sealed record ProfileDto(
    string Name,
    string Title,
    string ShortBio,
    string AvatarInitials,
    IReadOnlyList<string> SuggestedQuestions);
