using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

// CV alignment (Scyko via Mega, approved 2026-09-29 09:15). Source: Silas-Wong-CV.pdf.
public class CvAlignmentTests
{
    private static string Root => TestSupport.FindKnowledgePath();
    private static string Fact(string name) => File.ReadAllText(Path.Combine(Root, "facts", name));
    private static string Build(string q) => new PromptBuilder().BuildSystem("Silas Wong", [], null, q);

    private static IEnumerable<string> AllText()
    {
        foreach (var f in Directory.GetFiles(Root, "*.md", SearchOption.AllDirectories)) yield return File.ReadAllText(f);
        var src = Path.Combine(Directory.GetParent(Root)!.FullName, "src");
        foreach (var f in Directory.GetFiles(src, "*.cs", SearchOption.AllDirectories))
            if (!f.Contains("/bin/") && !f.Contains("/obj/")) yield return File.ReadAllText(f);
    }

    // 1. IBM RAG and Agentic AI is In progress, never obtained / completed. Data Engineering unchanged.
    [Fact]
    public void Item1_ibm_rag_certificate_is_in_progress()
    {
        var edu = Fact("education.md");
        Assert.Contains("Status: In progress", edu);
        Assert.Contains("Completed 16 February 2024. Professional Certificate in Data Engineering Specialization, IBM.", edu);
        Assert.DoesNotContain("No date is listed on this certificate", edu);
        Assert.Contains("in progress", Fact("extra-experience.md"));
        Assert.True(PromptBuilder.LooksLikeCertifications("What certifications do you have?"));
        Assert.False(PromptBuilder.LooksLikeCertifications("Do you have an IELTS certificate?"));
        Assert.Contains(PromptBuilder.CertificationsDirective, Build("What certifications do you have?"));
        Assert.Contains("The IBM Professional Certificate in RAG and Agentic AI is in progress.", PromptBuilder.CertificationsDirective);
        Assert.Contains("In progress; never say it is obtained or completed", PromptBuilder.HardBiographyDirective);
        var completed = new System.Text.RegularExpressions.Regex(@"(?:completed|obtained|earned)\s+(?:the\s+)?(?:IBM\s+)?(?:Professional\s+Certificate\s+in\s+)?RAG and Agentic AI", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var t in AllText()) Assert.DoesNotMatch(completed, t);
    }

    [Fact]
    public void Item1_guard_rewrites_completed_rag_certificate()
    {
        Assert.Equal("I have the IBM Data Engineering certificate. The IBM Professional Certificate in RAG and Agentic AI is in progress.",
            BiographyGuard.Sanitize("I have the IBM Data Engineering certificate. I also completed the IBM Professional Certificate in RAG and Agentic AI."));
        var ok = "The IBM Professional Certificate in RAG and Agentic AI is in progress.";
        Assert.Equal(ok, BiographyGuard.Sanitize(ok));
    }

    // 2. TradeLink: .NET Framework only, no React.
    [Fact]
    public void Item2_tradelink_is_dotnet_framework_only()
    {
        var tl = Fact("tradelink.md");
        Assert.Contains("Developed and maintained web-based applications (.NET Framework) to support user requirements and improve system functionality.", tl);
        Assert.DoesNotContain(".NET Framework, React", tl);
        Assert.DoesNotContain("Programmer, 程式設計師, .NET Framework, React", tl);
        Assert.DoesNotContain(".NET Framework, React", PromptBuilder.HardBiographyDirective);
        foreach (var t in AllText()) Assert.DoesNotContain("(.NET Framework, React)", t);
        Assert.Equal("At TradeLink I built web applications in .NET Framework.",
            BiographyGuard.Sanitize("At TradeLink I built web applications in .NET Framework and React."));
    }

    // TradeLink first answer names .NET Framework (Scyko via Mega, 2026-09-29 09:23).
    [Fact]
    public void TradeLink_first_answer_names_dotnet_framework_not_react()
    {
        const string q = "What did you do at TradeLink?";
        Assert.True(PromptBuilder.LooksLikeTradeLinkGeneric(q));
        Assert.False(PromptBuilder.LooksLikeTradeLinkGeneric("Why did you leave TradeLink?"));
        Assert.Contains(PromptBuilder.TradeLinkGenericDirective, Build(q));
        var answer = PromptBuilder.TradeLinkGenericDirective.Split('"')[1].Replace("\\", "");
        Assert.Contains("At TradeLink I was a Programmer, working on web-based applications in .NET Framework and the portal backend.", PromptBuilder.TradeLinkGenericDirective);
        Assert.Contains(".NET Framework", answer);
        Assert.DoesNotContain("React", answer);
        var tl = Fact("tradelink.md");
        var instr = tl.Split('\n').First(l => l.StartsWith("When asked generally what I did at TradeLink"));
        Assert.Contains(".NET Framework", instr);
        Assert.DoesNotContain("React", instr);
    }

    // 3. Agentic AI skills; SDD / HITL / AI-native bans lifted; MCP connecting only.
    [Fact]
    public void Item3_agentic_ai_skills_and_bans_lifted()
    {
        const string list = "AI-native SDLC, Spec-Driven Development, Human-in-the-Loop, Context Engineering, Context as Code, MCP, and RAG";
        Assert.Contains(list, PromptBuilder.AiSkillsDirective);
        Assert.Contains("Do not invent project stories", PromptBuilder.AiSkillsDirective);
        Assert.True(PromptBuilder.LooksLikeAiSkills("What AI skills do you have?"));
        Assert.Contains(PromptBuilder.AiSkillsDirective, Build("What AI skills do you have?"));
        Assert.Contains("Mention them only when they ask about AI skills or it is relevant", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Context as Code", Fact("skills.md"));
        Assert.Contains("do not invent project stories", Fact("ai-practice.md"));
        Assert.DoesNotContain("CV Skills AI evidence lock: answer only with the agentic CLI, RAG, and context engineering", Fact("ai-practice.md"));

        var banned = PromptBuilder.SpokenStyleDirective.Split('\n').First(l => l.Contains("Words that never appear"));
        var tone = File.ReadAllText(Path.Combine(Root, "tone", "professional.md")).Split('\n').First(l => l.Contains("Words that never appear"));
        foreach (var line in new[] { banned, tone })
        {
            foreach (var lifted in new[] { "SDD", "human-in-the-loop", "Spec-Driven", "AI-native" })
                Assert.DoesNotContain(lifted, line, StringComparison.OrdinalIgnoreCase);
            foreach (var kept in new[] { "XI", "sub-agents", "orchestrator", "arc", "journey" })
                Assert.Contains(kept, line);
        }
        var s = "I use Spec-Driven Development and Human-in-the-Loop in an AI-native SDLC.";
        Assert.Equal(s, BiographyGuard.Sanitize(s));
    }

    [Fact]
    public void Item3_mcp_is_connecting_existing_servers_only()
    {
        Assert.True(PromptBuilder.LooksLikeMcp("Have you used MCP?"));
        Assert.False(PromptBuilder.LooksLikeMcp("What did you do at HAECO?"));
        Assert.Contains(PromptBuilder.McpDirective, Build("Have you used MCP?"));
        Assert.Contains("I've connected existing MCP servers to an agent.", PromptBuilder.McpDirective);
        Assert.Equal("Yes. I've connected existing MCP servers to an agent.",
            BiographyGuard.Sanitize("Yes. I built an MCP server for our team."));
        Assert.Equal("I've connected existing MCP servers to an agent.",
            BiographyGuard.Sanitize("I wrote my own MCP server in C#."));
        var ok = "Yes. I've connected existing MCP servers to an agent.";
        Assert.Equal(ok, BiographyGuard.Sanitize(ok));
        foreach (var t in AllText())
        {
            Assert.DoesNotContain("I built an MCP server", t, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("I wrote an MCP server", t, StringComparison.OrdinalIgnoreCase);
        }
    }

    // 4. Backend: gRPC listed only.
    [Fact]
    public void Item4_backend_lists_grpc()
    {
        Assert.Contains(".NET Core, C#, RESTful APIs, gRPC", Fact("skills.md"));
        Assert.Contains("If they ask only about backend: .NET Core, C#, RESTful APIs, gRPC.", PromptBuilder.TechStackDirective);
        Assert.Contains("do not invent a project or detail", PromptBuilder.TechStackDirective);
        Assert.Contains("gRPC", PromptBuilder.HardBiographyDirective);
        Assert.True(PromptBuilder.LooksLikeTechStack("What backend technologies do you know?"));
        Assert.Contains(PromptBuilder.TechStackDirective, Build("What backend technologies do you know?"));
    }

    // 5. standing_locks: Read & Sign is UAT (not DEV); the AI-native ban lock is gone.
    [Fact]
    public void Item5_read_and_sign_uat_and_no_ai_native_ban()
    {
        var rx = new System.Text.RegularExpressions.Regex(@"Read (?:and|&) Sign[^.]{0,60}\bDEV\b|R&S DEV", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var t in AllText())
        {
            Assert.DoesNotMatch(rx, t);
            Assert.DoesNotContain("Do not use AI-native", t, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Contains("Read and Sign is at the UAT", PromptBuilder.HardBiographyDirective + PromptBuilder.SpokenStyleDirective + File.ReadAllText(Path.Combine(Directory.GetParent(Root)!.FullName, "src", "InterviewMe.Application", "Chat", "PromptBuilder.cs")));
    }

    // Locks that must survive.
    [Fact]
    public void Existing_locks_kept()
    {
        Assert.Contains("At HAECO I'm an Assistant Solution Analyst on MRO engineering IT solutions. I cover the full cycle from requirements and specs through full-stack delivery, UAT, go-live and support, and on some work I partner with our Mainland team more as a technical BA.", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("Towing is the system that moves aircraft between bays.", PromptBuilder.TowingDirective);
    }
}
