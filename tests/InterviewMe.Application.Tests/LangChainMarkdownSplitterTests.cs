using InterviewMe.Infrastructure.Knowledge;

namespace InterviewMe.Application.Tests;

public class LangChainMarkdownSplitterTests
{
    [Fact]
    public void Splits_markdown_on_headings_with_overlap()
    {
        var markdown = """
            # Projects
            intro
            ## Harborline Ops Board
            ferry dashboard
            ## Lumen Desk
            notes app
            """;

        var chunks = LangChainMarkdownSplitter.SplitFile("projects.md", markdown);

        Assert.NotEmpty(chunks);
        Assert.Contains(chunks, c => c.Text.Contains("ferry", System.StringComparison.OrdinalIgnoreCase)
                                     || c.Title.Contains("Harborline", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(chunks, c => c.Title.Contains("Lumen", System.StringComparison.OrdinalIgnoreCase)
                                     || c.Text.Contains("notes", System.StringComparison.OrdinalIgnoreCase));
        Assert.All(chunks, c => Assert.Equal("projects.md", c.Source));
    }
}
