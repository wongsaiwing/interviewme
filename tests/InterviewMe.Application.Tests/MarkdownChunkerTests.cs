using InterviewMe.Application.Knowledge;

namespace InterviewMe.Application.Tests;

public class MarkdownChunkerTests
{
    [Fact]
    public void Splits_markdown_on_headings()
    {
        var markdown = """
            # Projects
            intro
            ## Harborline Ops Board
            ferry dashboard
            ## Lumen Desk
            notes app
            """;

        var chunks = MarkdownChunker.ChunkFile("projects.md", markdown);

        Assert.Equal(3, chunks.Count);
        Assert.Contains(chunks, c => c.Title == "Harborline Ops Board" && c.Text.Contains("ferry"));
        Assert.Contains(chunks, c => c.Title == "Lumen Desk");
        Assert.All(chunks, c => Assert.Equal("projects.md", c.Source));
    }
}
