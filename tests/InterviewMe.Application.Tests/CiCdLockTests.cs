using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// CI/CD lock (Scyko via Mega, 2026-10-05 08:03): Git only, never CI/CD.
public class CiCdLockTests
{
    private static string Root => TestSupport.FindKnowledgePath();
    private static string Fact(string name) => File.ReadAllText(Path.Combine(Root, "facts", name));
    private static string Build(string q) => new PromptBuilder().BuildSystem("Silas Wong", [], null, q);
    private const string Answer = "No, I haven't worked with CI/CD.";

    [Theory]
    [InlineData("Have you worked with CI/CD?")]
    [InlineData("Do you have experience with continuous integration?")]
    public void CiCd_question_routes_to_denial(string q)
    {
        Assert.True(PromptBuilder.LooksLikeCiCd(q));
        Assert.Contains(PromptBuilder.CiCdDirective, Build(q));
        Assert.Contains($"\"{Answer}\" Then stop.", PromptBuilder.CiCdDirective);
        Assert.Contains("CI/CD: never done; Git only.", PromptBuilder.HardBiographyDirective);
    }

    [Fact]
    public void Knowledge_has_no_cicd_claims()
    {
        Assert.Equal("React, React Native, TypeScript. .NET Core, C#, RESTful APIs, gRPC. MSSQL, MongoDB. Git.", Fact("skills.md").Split('\n')[2]);
        Assert.DoesNotContain("Git, CI/CD", Fact("skills.md"));
        var swc = Fact("swc.md");
        Assert.DoesNotContain("CI and CD", swc);
        Assert.Contains("Data visualization design and implementation. Source control.", swc);
        Assert.DoesNotContain("CI/CD", PromptBuilder.TechStackDirective);
        // Data pipelines at TradeLink stay.
        Assert.Contains("data pipelines (ETL, SSIS)", Fact("tradelink.md"));
    }

    [Theory]
    [InlineData("Yes, I've set up CI/CD pipelines.", Answer)]
    [InlineData("At HAECO I used Git on Azure DevOps. I also did continuous integration.", "At HAECO I used Git on Azure DevOps. " + Answer)]
    [InlineData("I worked on CI and CD pipelines at Small World Consulting.", Answer)]
    public void Guard_rewrites_affirmative_cicd(string raw, string expected) => Assert.Equal(expected, BiographyGuard.Sanitize(raw));

    [Theory]
    [InlineData(Answer)]
    [InlineData("I built data pipelines with ETL and SSIS.")]
    [InlineData("Yes, at HAECO I used Git on Azure DevOps.")]
    public void Guard_keeps_denial_and_data_pipelines(string s) => Assert.Equal(s, BiographyGuard.Sanitize(s));

    [Fact]
    public void Other_locks_unchanged()
    {
        Assert.Contains("\"Yes, at HAECO I used Git on Azure DevOps.\"", PromptBuilder.GitDirective);
        Assert.Contains("https://github.com/wongsaiwing/interviewme", PromptBuilder.GitHubDirective);
        Assert.Contains("\"No, I haven't used Jira.\"", PromptBuilder.NeverUsedToolsDirective);
        Assert.Contains("\"Yes, I've worked in Agile.\"", PromptBuilder.AgileDirective);
        Assert.Contains("At TradeLink I was a Programmer, working on web-based applications in .NET Framework and the portal backend.", PromptBuilder.TradeLinkGenericDirective);
        Assert.Contains("At HAECO I'm an Assistant Solution Analyst on MRO engineering IT solutions.", PromptBuilder.HaecoGenericDirective);
        Assert.False(PromptBuilder.LooksLikeCiCd("Have you used Git?"));
    }
}
