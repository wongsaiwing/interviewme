using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// CI/CD lock (Scyko via Mega, 2026-10-05 08:08): used existing pipelines at HAECO and SWC;
// never built or configured them; none at TradeLink.
public class CiCdLockTests
{
    private static string Root => TestSupport.FindKnowledgePath();
    private static string Fact(string name) => File.ReadAllText(Path.Combine(Root, "facts", name));
    private static string Build(string q) => new PromptBuilder().BuildSystem("Silas Wong", [], null, q);
    private const string Answer = "Yes. At HAECO I used the existing CI/CD pipelines that our DevOps engineers set up, and in my Small World Consulting internship I also used existing pipelines.";
    private const string SetupAnswer = "No, I didn't set them up. I used existing CI/CD pipelines at HAECO and at Small World Consulting.";
    private const string Used = "At HAECO I used the existing CI/CD pipelines that our DevOps engineers set up, and in my Small World Consulting internship I also used existing pipelines.";

    [Theory]
    [InlineData("Have you worked with CI/CD?")]
    [InlineData("Do you have experience with continuous integration?")]
    public void CiCd_question_gets_used_existing_answer(string q)
    {
        Assert.True(PromptBuilder.LooksLikeCiCd(q));
        Assert.False(PromptBuilder.LooksLikeCiCdSetup(q));
        Assert.Contains(PromptBuilder.CiCdDirective, Build(q));
        Assert.Contains($"\"{Answer}\" Then stop.", PromptBuilder.CiCdDirective);
    }

    [Theory]
    [InlineData("Did you set up CI/CD pipelines?")]
    [InlineData("Have you built or configured a CI/CD pipeline?")]
    public void Setup_question_gets_denial_plus_used(string q)
    {
        Assert.True(PromptBuilder.LooksLikeCiCdSetup(q));
        var system = Build(q);
        Assert.Contains(PromptBuilder.CiCdSetupDirective, system);
        Assert.DoesNotContain(PromptBuilder.CiCdDirective, system);
        Assert.Contains($"\"{SetupAnswer}\" Then stop.", PromptBuilder.CiCdSetupDirective);
    }

    [Fact]
    public void Prompt_and_knowledge_match_the_lock()
    {
        Assert.Contains("Never built, set up, configured, or maintained them; no tool names, no details. None at TradeLink.", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Never say I built, set up, configured, or maintained pipelines.", PromptBuilder.CiCdDirective);
        Assert.DoesNotContain("No, I haven't worked with CI/CD", PromptBuilder.CiCdDirective + Fact("skills.md"));
        var skills = Fact("skills.md");
        Assert.Equal("React, React Native, TypeScript. .NET Core, C#, RESTful APIs, gRPC. MSSQL, MongoDB. Git.", skills.Split('\n')[2]);
        Assert.Contains(Answer, skills);
        Assert.Contains(SetupAnswer, skills);
        Assert.Contains("None at TradeLink.", skills);
        var swc = Fact("swc.md");
        Assert.Contains("Used existing CI/CD pipelines built by others (did not build or configure them).", swc);
        Assert.Contains("Data visualization design and implementation. Source control.", swc);
        Assert.DoesNotContain("CI and CD pipelines", swc);
        Assert.DoesNotContain("CI", PromptBuilder.ExtraExperienceDirective); // SWC first answer unchanged, no CI/CD volunteered
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"CI/CD|continuous integration", System.Text.RegularExpressions.RegexOptions.IgnoreCase), Fact("tradelink.md"));
        Assert.DoesNotContain("CI/CD", PromptBuilder.TechStackDirective);
        foreach (var t in new[] { PromptBuilder.CiCdDirective.Split('"')[1], PromptBuilder.CiCdSetupDirective.Split('"')[1] })
            foreach (var tool in new[] { "Azure", "Jenkins", "GitHub Actions", "GitLab" })
                Assert.DoesNotContain(tool, t);
    }

    [Theory]
    [InlineData("I set up the CI/CD pipelines at HAECO.", Used)]
    [InlineData("I configured and maintained our CI/CD pipelines. I also wrote specs.", Used + " I also wrote specs.")]
    [InlineData("I worked on the portal backend. At TradeLink I used CI/CD pipelines.", "I worked on the portal backend.")]
    public void Guard_rewrites_build_claims_and_strips_tradelink(string raw, string expected) => Assert.Equal(expected, BiographyGuard.Sanitize(raw));

    [Theory]
    [InlineData(Answer)]
    [InlineData(SetupAnswer)]
    [InlineData("At HAECO I used existing CI/CD pipelines.")]
    [InlineData("In my Small World Consulting internship I also used existing CI/CD pipelines.")]
    [InlineData("I built data pipelines with ETL and SSIS.")]
    [InlineData("Yes, at HAECO I used Git on Azure DevOps.")]
    public void Guard_keeps_used_existing_and_data_pipelines(string s) => Assert.Equal(s, BiographyGuard.Sanitize(s));

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
        Assert.False(PromptBuilder.LooksLikeCiCd("Did you build data pipelines at TradeLink?"));
    }
}
