namespace InterviewMe.Application.Profile;

public sealed class ProfileOptions
{
    public const string SectionName = "Profile";

    public string Name { get; set; } = "Silas Wong";
    public string Title { get; set; } = "Assistant Business Analyst, HAECO";
    public string ShortBio { get; set; } =
        "Full-stack developer and business analyst in Hong Kong. Requirements through delivery on MRO systems.";
    public string AvatarInitials { get; set; } = "SW";
    public List<string> SuggestedQuestions { get; set; } = [];
}

public sealed record ProfileDto(
    string Name,
    string Title,
    string ShortBio,
    string AvatarInitials,
    IReadOnlyList<string> SuggestedQuestions);
