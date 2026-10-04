using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// Git lock (Scyko via Mega, 2026-10-05 07:52, corrected 07:53): Git at HAECO, hosted on Azure DevOps; not at TradeLink.
// Azure DevOps use is limited to the Git repo.
public class GitLockTests
{
    private static string Root => TestSupport.FindKnowledgePath();
    private static string Fact(string name) => File.ReadAllText(Path.Combine(Root, "facts", name));
    private static string Build(string q) => new PromptBuilder().BuildSystem("Silas Wong", [], null, q);
    private const string Answer = "Yes, at HAECO I used Git on Azure DevOps.";

    [Theory]
    [InlineData("Have you used Git?")]
    [InlineData("Have you used Azure DevOps?")]
    public void Git_and_azure_devops_questions_say_haeco(string q)
    {
        Assert.True(PromptBuilder.LooksLikeGit(q));
        Assert.False(PromptBuilder.LooksLikeNeverUsedTools(q));
        var system = Build(q);
        Assert.Contains(PromptBuilder.GitDirective, system);
        Assert.DoesNotContain(PromptBuilder.NeverUsedToolsDirective, system);
        Assert.Contains($"\"{Answer}\" Then stop.", PromptBuilder.GitDirective);
        Assert.Contains("never claim Boards, Pipelines, Artifacts, Test Plans", PromptBuilder.GitDirective);
    }

    [Fact]
    public void GitHub_questions_keep_their_route()
    {
        Assert.False(PromptBuilder.LooksLikeGit("Do you have a GitHub?"));
        Assert.True(PromptBuilder.LooksLikeGitHub("Do you have a GitHub?"));
        Assert.Contains("https://github.com/wongsaiwing/interviewme", PromptBuilder.GitHubDirective);
    }

    [Fact]
    public void TradeLink_has_no_git()
    {
        var tl = Fact("tradelink.md");
        Assert.DoesNotContain("Source control (Git)", tl);
        Assert.Contains("Functional and system integration, and UAT.", tl);
        Assert.DoesNotContain("SSRS, Git", tl);
        Assert.Contains("Do not say Git or React for TradeLink", tl);
        Assert.Contains("never say React or Git for TradeLink", PromptBuilder.TradeLinkGenericDirective);
        Assert.Contains("Never say Git for TradeLink", PromptBuilder.HardBiographyDirective);
        Assert.DoesNotContain("Git", PromptBuilder.TradeLinkGenericDirective.Split('"')[1]);
    }

    [Fact]
    public void Azure_devops_only_in_git_at_haeco_context()
    {
        Assert.DoesNotContain("Azure DevOps", PromptBuilder.TechStackDirective);
        Assert.DoesNotContain("Azure DevOps", Fact("skills.md").Split('\n')[2]);
        Assert.Contains("at HAECO I used Git on Azure DevOps; not at TradeLink", Fact("skills.md"));
        Assert.Contains("Git: at HAECO I used Git on Azure DevOps; not at TradeLink.", PromptBuilder.HardBiographyDirective);
        Assert.DoesNotContain("Azure DevOps", PromptBuilder.NeverUsedToolsDirective);
    }

    [Theory]
    [InlineData("I worked on the portal backend. At TradeLink I used Git for source control.", "I worked on the portal backend.")]
    [InlineData("At TradeLink I worked on the portal backend and used Git. I also did the database work.", "I also did the database work.")]
    [InlineData("I also used Azure Boards for work items.", "At HAECO I used Git on Azure DevOps.")]
    [InlineData("At HAECO I used Git on Azure DevOps, with Pipelines for CI/CD.", "At HAECO I used Git on Azure DevOps.")]
    public void Guard_strips_tradelink_git_and_azure_devops_features(string raw, string expected)
        => Assert.Equal(expected, BiographyGuard.Sanitize(raw));

    [Theory]
    [InlineData("Yes, at HAECO I used Git on Azure DevOps.")]
    [InlineData("At HAECO I used Git on Azure DevOps.")]
    [InlineData("Yes, I've used Git.")]
    [InlineData("The public repo is InterviewMe on GitHub.")]
    public void Guard_does_not_deny_git_or_azure_devops(string s) => Assert.Equal(s, BiographyGuard.Sanitize(s));

    [Fact]
    public void Other_locks_unchanged()
    {
        Assert.Contains("\"Yes, I've worked in Agile.\"", PromptBuilder.AgileDirective);
        Assert.Contains("\"No, I haven't used Jira.\"", PromptBuilder.NeverUsedToolsDirective);
        Assert.Contains("At TradeLink I was a Programmer, working on web-based applications in .NET Framework and the portal backend.", PromptBuilder.TradeLinkGenericDirective);
        Assert.Contains("If they ask only about backend: .NET Core, C#, RESTful APIs, gRPC.", PromptBuilder.TechStackDirective);
        Assert.Contains("GitHub Copilot CLI", PromptBuilder.WhichToolDirective);
        Assert.Contains("At HAECO I'm an Assistant Solution Analyst on MRO engineering IT solutions.", PromptBuilder.HaecoGenericDirective);
    }
}
