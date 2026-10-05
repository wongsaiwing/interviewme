using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// Shift Briefing lock (Scyko via Mega, approved 2026-10-05 08:15; CV R9): 80% is frontend phase only; project not finished.
public class ShiftBriefingFrontendTests
{
    private static string Root => TestSupport.FindKnowledgePath();
    private static string Build(string q) => new PromptBuilder().BuildSystem("Silas Wong", [], null, q);

    private static IEnumerable<(string Name, string Text)> AllText()
    {
        foreach (var f in Directory.GetFiles(Root, "*.md", SearchOption.AllDirectories)) yield return (f, File.ReadAllText(f));
        var src = Path.Combine(Directory.GetParent(Root)!.FullName, "src");
        foreach (var f in Directory.GetFiles(src, "*.cs", SearchOption.AllDirectories))
            if (!f.Contains("/bin/") && !f.Contains("/obj/")) yield return (f, File.ReadAllText(f));
    }

    [Fact]
    public void Default_answers_scope_80_percent_to_frontend()
    {
        var sb = PromptBuilder.ShiftBriefingDirective;
        Assert.Contains("I built it with an AI-native SDLC using an agentic CLI, and it cut frontend delivery time and man-hour cost by 80%. It's at the UAT stage now.", sb);
        Assert.Contains("I built Shift Briefing with an AI-native SDLC, using an agentic CLI for the .NET and React development. It cut frontend delivery time and man-hour cost by 80%, and it's at the UAT stage.", sb);
        Assert.Contains("On Shift Briefing, the AI-native SDLC cut frontend delivery time and man-hour cost by 80%. I used an agentic CLI for the .NET and React work. It's at the UAT stage.", sb);
        Assert.Contains("Never say the whole project or delivery saved 80%.", sb);
        Assert.Contains("Cut frontend delivery time and man-hour cost by 80% by adopting an AI-native SDLC.", File.ReadAllText(Path.Combine(Root, "facts", "haeco.md")));
    }

    [Fact]
    public void Estimate_and_cost_follow_ups_are_frontend_scoped()
    {
        Assert.Contains("\"For the frontend, the original estimate was 50 man-days, and with the AI-native SDLC it took 10. That's where the 80% comes from, for both delivery time and man-hour cost.\"", PromptBuilder.ShiftBriefingEstimateDirective);
        Assert.Contains("\"For the frontend phase, the token cost was HK$200 a day over 10 days, so HK$2,000. At HK$1,000 per person per day, the 40 man-days saved are worth HK$40,000, so the net savings were HK$38,000.\"", PromptBuilder.ShiftBriefingCostDirective);
        var haeco = File.ReadAllText(Path.Combine(Root, "facts", "haeco.md"));
        Assert.Contains("All of these numbers are for the frontend phase only.", haeco);
        Assert.DoesNotContain("frontend and backend to UAT", haeco);
        Assert.DoesNotContain("frontend + backend", File.ReadAllText(Path.Combine(Root, "facts", "haeco-projects.md")));
    }

    [Fact]
    public void No_unscoped_80_percent_wording_anywhere()
    {
        var rx = new System.Text.RegularExpressions.Regex(@"(?<!frontend )delivery time by 80%|(?<!frontend )delivery time and man-hour cost by 80%", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var (name, text) in AllText())
            foreach (System.Text.RegularExpressions.Match m in rx.Matches(text))
                Assert.Fail($"{name}: {m.Value}");
    }

    [Fact]
    public void Finished_question_says_still_in_progress()
    {
        const string q = "Is Shift Briefing finished?";
        Assert.True(PromptBuilder.LooksLikeShiftBriefing(q));
        Assert.True(PromptBuilder.LooksLikeSbFinishedQuestion(q));
        Assert.Contains(PromptBuilder.ShiftBriefingFinishedDirective, Build(q));
        Assert.Contains("\"No, the project is still in progress. It's at the UAT stage.\"", PromptBuilder.ShiftBriefingFinishedDirective);
        Assert.DoesNotContain("Feature-complete", PromptBuilder.ShiftBriefingDirective);
        Assert.Contains("still in progress", File.ReadAllText(Path.Combine(Root, "facts", "production.md")));
    }

    [Theory]
    [InlineData("On Shift Briefing it cut delivery time by 80% and man-hour cost by 80%.", "On Shift Briefing it cut frontend delivery time and man-hour cost by 80%.")]
    [InlineData("Shift Briefing cut delivery time by 80%.", "Shift Briefing cut frontend delivery time by 80%.")]
    [InlineData("The AI-native SDLC reduced delivery time and man-hour cost by 80%.", "The AI-native SDLC reduced frontend delivery time and man-hour cost by 80%.")]
    [InlineData("Shift Briefing is finished and live now.", "Shift Briefing is still in progress. It's at the UAT stage.")]
    public void Guard_scopes_and_unfinishes(string raw, string expected) => Assert.Equal(expected, BiographyGuard.Sanitize(raw));

    [Theory]
    [InlineData("It cut frontend delivery time and man-hour cost by 80%, and it's at the UAT stage.")]
    [InlineData("No, the project is still in progress. It's at the UAT stage.")]
    [InlineData("Shift Briefing isn't finished yet.")]
    public void Guard_keeps_scoped_and_in_progress(string s) => Assert.Equal(s, BiographyGuard.Sanitize(s));

    [Fact]
    public void Other_locks_unchanged()
    {
        Assert.Contains("At HAECO I'm an Assistant Solution Analyst on MRO engineering IT solutions. I cover the full cycle from requirements and specs through full-stack delivery, UAT, go-live and support, and on some work I partner with our Mainland team more as a technical BA.", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("Towing is the system that moves aircraft between bays.", PromptBuilder.TowingDirective);
        Assert.Contains("which covers more than 70 departments company-wide, and I worked with more than 10 stakeholders on it", PromptBuilder.StakeholderCountDirective);
        Assert.Contains("HKD 35,000 per month.", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("\"Yes, at HAECO I used Git on Azure DevOps.\"", PromptBuilder.GitDirective);
        Assert.Contains("Yes. At HAECO I used the existing CI/CD pipelines", PromptBuilder.CiCdDirective);
        Assert.Contains("It's in UAT right now. We're working through UAT, and production comes after that.", PromptBuilder.ShiftBriefingDirective);
        Assert.Contains("HK$2,000", PromptBuilder.ShiftBriefingCostDirective);
        Assert.DoesNotContain("50 man-days", PromptBuilder.ShiftBriefingDirective);
    }
}
