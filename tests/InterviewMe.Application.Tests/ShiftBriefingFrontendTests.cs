using InterviewMe.Application.Chat;
using InterviewMe.Domain;

namespace InterviewMe.Application.Tests;

// Official CV 2026-10-12: AI-native SDLC 83%/81% (baseline prior manual); SB in active development; no frontend-only 80%.
public class ShiftBriefingFrontendTests
{
    private static string Root => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "knowledge"));

    private static string Build(string q) =>
        new PromptBuilder().BuildSystem("Silas Wong", Array.Empty<RetrievedFact>(), null, q);

    [Fact]
    public void Default_answers_use_cv_ai_native_metrics_and_active_development()
    {
        var sb = PromptBuilder.ShiftBriefingDirective;
        Assert.Contains("still in active development", sb);
        Assert.Contains("83%", sb);
        Assert.Contains("81%", sb);
        Assert.DoesNotContain("frontend delivery time and man-hour cost by 80%", sb);
        Assert.Contains("cutting delivery time by 83% and man-hour cost by 81%", File.ReadAllText(Path.Combine(Root, "facts", "haeco.md")));
    }

    [Fact]
    public void Estimate_and_cost_follow_ups_use_cv_baseline()
    {
        Assert.Contains("prior manual workflow", PromptBuilder.ShiftBriefingEstimateDirective);
        Assert.Contains("83%", PromptBuilder.ShiftBriefingEstimateDirective);
        Assert.Contains("81%", PromptBuilder.ShiftBriefingCostDirective);
        Assert.DoesNotContain("50 man-days", PromptBuilder.ShiftBriefingEstimateDirective);
        Assert.DoesNotContain("HK$2,000", PromptBuilder.ShiftBriefingCostDirective);
    }

    [Theory]
    [InlineData("On Shift Briefing it cut delivery time by 80% and man-hour cost by 80%.", "delivery time by 83% and man-hour cost by 81%")]
    [InlineData("Shift Briefing cut delivery time by 80%.", "delivery time by 83%")]
    [InlineData("The AI-native SDLC reduced delivery time and man-hour cost by 80%.", "delivery time by 83% and man-hour cost by 81%")]
    public void Guard_rewrites_retired_80_to_cv_metrics(string raw, string expectedFragment)
    {
        var cleaned = BiographyGuard.Sanitize(raw);
        Assert.Contains(expectedFragment, cleaned);
        Assert.DoesNotContain("80%", cleaned);
    }

    [Theory]
    [InlineData("It cut delivery time by 83% and man-hour cost by 81%, and it's still in active development.")]
    public void Guard_keeps_cv_metrics(string s)
    {
        Assert.Equal(s, BiographyGuard.Sanitize(s));
    }

    [Fact]
    public void Other_locks_unchanged()
    {
        Assert.Contains("At HAECO I'm an Assistant Business Analyst on MRO engineering IT solutions.", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("more than 10 stakeholders", PromptBuilder.StakeholderCountDirective);
        Assert.Contains("It's still in active development.", PromptBuilder.ShiftBriefingDirective);
    }

    [Theory]
    [InlineData("Is Shift Briefing finished?")]
    [InlineData("Is Shift Briefing done?")]
    public void Finished_routes_to_active_development(string q)
    {
        Assert.True(PromptBuilder.LooksLikeShiftBriefing(q));
        Assert.Contains(PromptBuilder.ShiftBriefingFinishedDirective, Build(q));
        Assert.Contains("\"No, the project is still in active development.\"", PromptBuilder.ShiftBriefingFinishedDirective);
    }
}
