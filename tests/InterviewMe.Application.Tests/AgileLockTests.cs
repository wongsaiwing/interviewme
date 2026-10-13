using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// Agile lock (Scyko via Mega, approved 2026-10-05 07:44). InterviewMe only; CV unchanged.
public class AgileLockTests
{
    private static string Root => TestSupport.FindKnowledgePath();
    private static string Fact(string name) => File.ReadAllText(Path.Combine(Root, "facts", name));
    private static string Build(string q) => new PromptBuilder().BuildSystem("Silas Wong", [], null, q);

    [Fact]
    public void Agile_answer_is_short_and_routed()
    {
        const string q = "Have you worked in Agile?";
        Assert.True(PromptBuilder.LooksLikeAgile(q));
        Assert.Contains(PromptBuilder.AgileDirective, Build(q));
        Assert.Contains("\"Yes, I've worked in Agile.\" Then stop.", PromptBuilder.AgileDirective);
        Assert.Contains("I have worked in Agile", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Yes, I've worked in Agile.", Fact("skills.md"));
        var s = "Yes, I've worked in Agile.";
        Assert.Equal(s, BiographyGuard.Sanitize(s));
    }

    [Fact]
    public void No_scrum_sprints_ceremonies_in_agile_answer()
    {
        var answer = PromptBuilder.AgileDirective.Split('"')[1];
        foreach (var w in new[] { "Scrum", "sprint", "ceremon", "Jira", "Confluence", "Azure DevOps", "training" })
            Assert.DoesNotContain(w, answer, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Never_used_tools_are_not_claimed_in_prompts_or_knowledge()
    {
        foreach (var q in new[] { "Have you used Jira?", "Do you use Confluence?", "Have you done user training?" })
        {
            Assert.True(PromptBuilder.LooksLikeNeverUsedTools(q), q);
            Assert.Contains(PromptBuilder.NeverUsedToolsDirective, Build(q));
        }
        Assert.Contains("\"No, I haven't used Jira.\"", PromptBuilder.NeverUsedToolsDirective);
        Assert.DoesNotContain("Azure DevOps", PromptBuilder.TechStackDirective);
        Assert.DoesNotContain("Git, Azure DevOps", PromptBuilder.HardBiographyDirective);
        Assert.DoesNotContain("Azure DevOps", Fact("skills.md").Split('\n')[2]);
        Assert.DoesNotContain("DevOps", Fact("tradelink.md"));
        Assert.Contains("Never used: Jira, Confluence. Never did user training.", Fact("skills.md"));
        Assert.False(PromptBuilder.LooksLikeNeverUsedTools("Have you worked with Azure DevOps?")); // Azure DevOps un-denied (Scyko 2026-10-05 07:53)
    }

    [Theory]
    [InlineData("Yes, I've used Jira for tracking work.", "I haven't used Jira.")]
    [InlineData("I wrote pages in Confluence.", "I haven't used Confluence.")]
    [InlineData("At HAECO we track PBIs in Azure DevOps. It works well.", "At HAECO I used Git on Azure DevOps. It works well.")]
    [InlineData("I also ran user training before go-live.", "I haven't done user training.")]
    [InlineData("I trained users on the new system.", "I haven't done user training.")]
    public void Guard_never_claims_jira_confluence_azure_devops_or_user_training(string raw, string expected)
    {
        Assert.Equal(expected, BiographyGuard.Sanitize(raw));
    }

    [Theory]
    [InlineData("No, I haven't used Jira.")]
    [InlineData("I've never used Confluence.")]
    [InlineData("No, I haven't done user training.")]
    public void Guard_keeps_denials(string s) => Assert.Equal(s, BiographyGuard.Sanitize(s));

    [Fact]
    public void Other_locks_unchanged()
    {
        Assert.Contains("At HAECO I'm an Assistant Business Analyst on MRO engineering IT solutions. I elicit and analyse requirements for MRO modules, follow agile delivery through UAT, and on some modules I own both the BA and full-stack work end-to-end.", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("I built Shift Briefing with an AI-native SDLC, using an agentic CLI for the .NET and React development, and it's still in active development.", PromptBuilder.ShiftBriefingDirective);
        Assert.Contains("prior manual workflow", PromptBuilder.ShiftBriefingEstimateDirective);
        Assert.Contains("83%", PromptBuilder.ShiftBriefingCostDirective);
        Assert.Contains("Read and Sign is a system where staff read a document and then sign it off", PromptBuilder.ReadAndSignDirective);
        Assert.Contains("It depends on the module. The largest spanned more than 10 stakeholders", PromptBuilder.StakeholderCountDirective);
        Assert.Contains("Towing is the system that moves aircraft between bays. It has more integrations than the earlier systems I worked on. I worked out the requirements with a BA, then built it full-stack myself and took it through UAT to production.", PromptBuilder.TowingDirective);
        Assert.Contains("At TradeLink I was a Programmer. I developed and maintained web application backends in .NET Framework", PromptBuilder.TradeLinkGenericDirective);
        Assert.Contains("I have the IBM Professional Certificate in Data Engineering. The IBM Professional Certificate in RAG and Agentic AI is in progress.", PromptBuilder.CertificationsDirective);
        Assert.Contains("AI-native SDLC, Spec-Driven Development, Human-in-the-Loop, Context Engineering, MCP, and RAG", PromptBuilder.AiSkillsDirective);
        Assert.Contains("Yes. I've connected existing MCP servers to an agent.", PromptBuilder.McpDirective);
        Assert.Contains("HKD 35,000 per month.", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("July 2022", PromptBuilder.HardBiographyDirective);
        Assert.False(PromptBuilder.LooksLikeAgile("What did you do at HAECO?"));
        Assert.False(PromptBuilder.LooksLikeNeverUsedTools("What did you do at TradeLink?"));
        Assert.Contains(PromptBuilder.HaecoGenericDirective.Trim(), Build("What did you do at HAECO?"));
    }
}
