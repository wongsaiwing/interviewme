using InterviewMe.Application.Chat;
using InterviewMe.Domain;

namespace InterviewMe.Application.Tests;

// Official CV 2026-10-12: Shift Briefing is in active development; systems split is ABA four + ASA three.
public class ShiftBriefingDevTests
{
    private static string Build(string q) =>
        new PromptBuilder().BuildSystem("Silas Wong", Array.Empty<RetrievedFact>(), null, q);

    [Fact]
    public void Shift_Briefing_answers_say_still_in_active_development()
    {
        var sb = PromptBuilder.ShiftBriefingDirective;
        Assert.Contains("still in active development", sb);
        Assert.Contains("\"No, the project is still in active development.\" Then stop.", PromptBuilder.ShiftBriefingFinishedDirective);
        Assert.Contains(PromptBuilder.ShiftBriefingFinishedDirective, Build("Is Shift Briefing finished?"));
    }

    [Fact]
    public void Cv_systems_split_is_four_modules_and_three_microservices()
    {
        Assert.Contains("four MRO modules", PromptBuilder.HaecoSystemsDirective);
        Assert.Contains("three full-stack MRO microservices", PromptBuilder.HaecoSystemsDirective);
        Assert.DoesNotContain("I've delivered seven MRO and operations systems", PromptBuilder.HaecoSystemsDirective);
    }

    [Fact]
    public void Guard_keeps_read_and_sign_uat_and_dev_lines()
    {
        var s = "Read and Sign I've taken to UAT, and Shift Briefing is still in active development.";
        var cleaned = BiographyGuard.Sanitize(s);
        Assert.Contains("Read and Sign", cleaned);
        Assert.Contains("UAT", cleaned);
        Assert.Contains("active development", cleaned);
    }
}
