using InterviewMe.Application.Chat;
using InterviewMe.Infrastructure.Embeddings;
using InterviewMe.Infrastructure.Knowledge;
using InterviewMe.Infrastructure.VectorStore;
using LangChain.Schema;
using Microsoft.Extensions.Logging.Abstractions;

namespace InterviewMe.Application.Tests;

public class KnowledgeIngestorTests
{
    [Fact]
    public async Task Ingests_facts_but_never_tone_or_avery()
    {
        var store = new LangChainVectorStore();
        var embeddings = new HashingEmbeddingModel();
        var path = TestSupport.FindKnowledgePath();
        var ingestor = new MarkdownKnowledgeIngestor(
            embeddings,
            store,
            NullLogger<MarkdownKnowledgeIngestor>.Instance,
            path);

        await ingestor.IngestAsync();

        var factFiles = Directory.GetFiles(Path.Combine(path, "facts"), "*.md", SearchOption.AllDirectories);
        Assert.Contains(factFiles, f => Path.GetFileName(f).Contains("haeco", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(factFiles, f => Path.GetFileName(f).Contains("bio", StringComparison.OrdinalIgnoreCase));

        Assert.IsType<LangChainVectorStore>(store);
        Assert.IsAssignableFrom<Microsoft.Extensions.VectorData.VectorStoreCollection<string, LangChainDocumentRecord>>(
            store.Collection);

        var query = await embeddings.EmbedAsync("HAECO aviation", CancellationToken.None);
        var hits = await store.SearchAsync(query, "What did you do at HAECO?", 8);
        Assert.Contains(hits, h => h.Text.Contains("HAECO", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(hits, h => h.Text.Contains("Avery Chen", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(hits, h => h.Text.Contains("style only", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(hits, h => h.Source.Contains("professional.md", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Compathnion_facts_are_wristband_not_LeaveHomeSafe()
    {
        var path = TestSupport.FindKnowledgePath();
        var file = Path.Combine(path, "facts", "internship-compathnion.md");
        var markdown = File.ReadAllText(file);
        Assert.Contains("wristband", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("home-quarantine", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LeaveHomeSafe", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("安心出行", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("StayHomeSafe", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("居安抗疫", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Swc_facts_name_Mike_as_boss_and_not_graph_engineering()
    {
        var path = TestSupport.FindKnowledgePath();
        var file = Path.Combine(path, "facts", "swc.md");
        var markdown = File.ReadAllText(file);
        Assert.Contains("Mike Berners-Lee", markdown);
        Assert.Contains("did not work with Tim", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("graph engineering", markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Github_facts_are_only_InterviewMe()
    {
        var path = TestSupport.FindKnowledgePath();
        var markdown = File.ReadAllText(Path.Combine(path, "facts", "github.md"));
        Assert.Contains("https://github.com/wongsaiwing/interviewme", markdown);
        Assert.Contains("Do not invent other public experiments", markdown);
        Assert.DoesNotContain("portfolio of small tools", markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Current_package_is_not_tracked_in_public_facts()
    {
        var path = TestSupport.FindKnowledgePath();
        // File may exist locally for deploy, but CurrentPayDirective must not embed figures.
        Assert.DoesNotContain("24000", PromptBuilder.CurrentPayDirective);
        Assert.DoesNotContain("14.5", PromptBuilder.CurrentPayDirective);
    }

    [Fact]
    public void TradeLink_facts_are_web_only()
    {
        var path = TestSupport.FindKnowledgePath();
        var markdown = File.ReadAllText(Path.Combine(path, "facts", "tradelink.md"));
        Assert.Contains("web-based applications only", markdown);
        Assert.Contains("Do not say console apps", markdown);
        Assert.DoesNotContain("and console apps", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not say console apps", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Official title is Programmer", markdown);
        Assert.Contains("Do not change the job title to full-stack developer", markdown);
        Assert.Contains("Programmer", PromptBuilder.HardBiographyDirective);
        Assert.Contains("full-stack developer title", PromptBuilder.HardBiographyDirective);
        Assert.DoesNotContain("console apps", PromptBuilder.HardBiographyDirective.Replace("Do not say console apps", ""));
    }

    [Fact]
    public void Degree_class_facts_are_22_with_reason()
    {
        var path = TestSupport.FindKnowledgePath();
        var education = File.ReadAllText(Path.Combine(path, "facts", "education.md"));
        Assert.Contains("UK 2:2 (Lower Second)", education);
        Assert.Contains("interest-based", education);
        Assert.Contains("not careless studying", education);
        Assert.DoesNotContain("haven't covered grades", education, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("six projects", File.ReadAllText(Path.Combine(path, "facts", "haeco.md")), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Expected_salary_facts_are_band_only()
    {
        var path = TestSupport.FindKnowledgePath();
        var compensation = File.ReadAllText(Path.Combine(path, "facts", "compensation.md"));
        Assert.Contains("30,000 to 35,000", compensation);
        Assert.Contains("That is enough", compensation);
        Assert.DoesNotContain("WFH day", compensation);
        Assert.Contains("Do not mention WFH", compensation);
        var skills = File.ReadAllText(Path.Combine(path, "facts", "skills.md"));
        Assert.Contains("MSSQL and MongoDB are skills I have", skills);
        Assert.Contains("Do not invent that MSSQL is the main database", skills);
        var haeco = File.ReadAllText(Path.Combine(path, "facts", "haeco.md"));
        Assert.Contains("Do not invent conflict, sudden requirement bombs, or difficult-stakeholder stories", haeco);
        Assert.DoesNotContain("stakeholder delayed go-live", haeco, StringComparison.OrdinalIgnoreCase);
    }
}
