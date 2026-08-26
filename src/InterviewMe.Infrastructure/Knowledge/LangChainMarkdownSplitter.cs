using InterviewMe.Domain;
using LangChain.Splitters.Text;

namespace InterviewMe.Infrastructure.Knowledge;

/// <summary>
/// Heading-aware LangChain splitter used for facts RAG (tone files never pass through here).
/// </summary>
public static class LangChainMarkdownSplitter
{
    private static readonly MarkdownHeaderTextSplitter Headers = new(
        headersToSplitOn: ["#", "##"],
        includeHeaders: true);

    private static readonly RecursiveCharacterTextSplitter Overlap = new(
        separators: ["\n\n", "\n", ". ", " ", ""],
        chunkSize: 900,
        chunkOverlap: 80);

    public static IReadOnlyList<KnowledgeChunk> SplitFile(string path, string markdown)
    {
        var source = Path.GetFileName(path);
        var fallbackTitle = Path.GetFileNameWithoutExtension(path);
        var sections = Headers.SplitText(markdown ?? string.Empty);
        if (sections.Count == 0 && !string.IsNullOrWhiteSpace(markdown))
        {
            sections = Overlap.SplitText(markdown);
        }

        var chunks = new List<KnowledgeChunk>();
        var index = 0;
        foreach (var section in sections)
        {
            var title = TitleFromSection(section, fallbackTitle);
            foreach (var piece in Overlap.SplitText(section))
            {
                var text = piece.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                chunks.Add(new KnowledgeChunk(
                    Id: $"{source}#{index}",
                    Source: source,
                    Title: title,
                    Text: text));
                index++;
            }
        }

        return chunks;
    }

    private static string TitleFromSection(string section, string fallback)
    {
        var first = section.Replace("\r\n", "\n").Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.Length > 0);
        if (string.IsNullOrWhiteSpace(first))
        {
            return fallback;
        }

        var parts = first.Split(':');
        var title = parts[^1].Trim();
        return string.IsNullOrWhiteSpace(title) ? fallback : title;
    }
}
