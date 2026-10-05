using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// Lock (Scyko via Mega, approved 2026-10-05 08:24; CV R9): Shift Briefing is only in the dev environment,
// still in development, not UAT. 7-system split: 3 go-live, 2 requirements, 1 to UAT (Read & Sign), 1 in development (Shift Briefing).
public class ShiftBriefingDevTests
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
    public void Shift_Briefing_answers_say_still_in_development()
    {
        var sb = PromptBuilder.ShiftBriefingDirective;
        Assert.Contains("I built it with an AI-native SDLC using an agentic CLI, and it cut frontend delivery time and man-hour cost by 80%. It's still in development.\"", sb);
        Assert.Contains("On Shift Briefing, the AI-native SDLC cut frontend delivery time and man-hour cost by 80%. I used an agentic CLI for the .NET and React work. It's still in development.\"", sb);
        Assert.Contains("It cut frontend delivery time and man-hour cost by 80%, and it's still in development.\"", sb);
        Assert.Contains("Status (when they ask about its status or why it has not gone live): \"It's still in development.\"", sb);
        Assert.Contains("\"No, the project is still in development.\" Then stop.", PromptBuilder.ShiftBriefingFinishedDirective);
        Assert.Contains(PromptBuilder.ShiftBriefingFinishedDirective, Build("Is Shift Briefing finished?"));
        Assert.Contains("still in development (dev environment only, not UAT;", PromptBuilder.HardBiographyDirective);
        Assert.DoesNotContain("UAT", sb.Split('\n').First(l => l.Contains("What it is (first level")));
    }

    [Fact]
    public void Seven_system_split_has_one_uat_and_one_in_development()
    {
        var projects = File.ReadAllText(Path.Combine(Root, "facts", "haeco-projects.md"));
        Assert.Contains("Delivered 7 MRO and operations systems: 3 full-stack to go-live, 2 requirements, 1 to UAT, 1 in development.", projects);
        Assert.Contains("1 to UAT (Read and Sign), 1 in development (Shift Briefing)", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Read and Sign I've taken to UAT, and Shift Briefing is still in development.\" Then stop.", PromptBuilder.HaecoSystemsDirective);
        var rx = new System.Text.RegularExpressions.Regex(@"\b(?:2|two) (?:to|at) UAT\b|both at the UAT|Read and Sign and Shift Briefing, I've taken to UAT", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var (name, text) in AllText())
            Assert.False(rx.IsMatch(text), name);
    }

    [Fact]
    public void No_shift_briefing_uat_claims_in_knowledge_or_prompt()
    {
        var rx = new System.Text.RegularExpressions.Regex(@"Shift Briefing[^.;!?""]*(?:at the UAT stage|in UAT|UAT-ready|taken to UAT)");
        foreach (var (name, text) in AllText())
            foreach (System.Text.RegularExpressions.Match m in rx.Matches(text))
                Assert.Fail($"{name}: {m.Value}");
    }

    [Theory]
    [InlineData("Shift Briefing is at the UAT stage now.", "Shift Briefing is still in development.")]
    [InlineData("Shift Briefing is a pre-shift briefing system. It's at the UAT stage now.", "Shift Briefing is a pre-shift briefing system. It's still in development.")]
    [InlineData("Read and Sign and Shift Briefing are both at the UAT stage.", "Read and Sign is at the UAT stage, and Shift Briefing is still in development.")]
    [InlineData("Shift Briefing is live.", "Shift Briefing is still in development.")]
    [InlineData("Shift Briefing is finished.", "Shift Briefing is still in development.")]
    public void Guard_rewrites_shift_briefing_uat_finished_live(string raw, string expected) => Assert.Equal(expected, BiographyGuard.Sanitize(raw));

    [Theory]
    [InlineData("Read and Sign is at the UAT stage.")]
    [InlineData("I built it full-stack using an AI-native SDLC with an agentic CLI, and it's at the UAT stage.")]
    [InlineData("It's at the UAT stage now. Production comes after UAT.")]
    [InlineData("Read and Sign I've taken to UAT, and Shift Briefing is still in development.")]
    [InlineData("No, the project is still in development.")]
    public void Guard_keeps_read_and_sign_uat_and_dev_lines(string s) => Assert.Equal(s, BiographyGuard.Sanitize(s));

    [Fact]
    public void Other_locks_unchanged()
    {
        Assert.Contains("\"For the frontend phase, the token cost was HK$200 a day over 10 days, so HK$2,000. At HK$1,000 per person per day, the 40 man-days saved are worth HK$40,000, so the net savings were HK$38,000.\"", PromptBuilder.ShiftBriefingCostDirective);
        Assert.Contains("\"For the frontend, the original estimate was 50 man-days, and with the AI-native SDLC it took 10. That's where the 80% comes from, for both delivery time and man-hour cost.\"", PromptBuilder.ShiftBriefingEstimateDirective);
        Assert.Contains("at the UAT stage", PromptBuilder.ReadAndSignDirective);
        Assert.Contains("At HAECO I'm an Assistant Solution Analyst on MRO engineering IT solutions. I cover the full cycle from requirements and specs through full-stack delivery, UAT, go-live and support, and on some work I partner with our Mainland team more as a technical BA.", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("Towing is the system that moves aircraft between bays.", PromptBuilder.TowingDirective);
    }
}
