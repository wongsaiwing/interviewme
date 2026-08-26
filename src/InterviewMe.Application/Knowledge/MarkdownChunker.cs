using InterviewMe.Domain;

namespace InterviewMe.Application.Knowledge;

public static class MarkdownChunker
{
    public static IReadOnlyList<KnowledgeChunk> ChunkFile(string path, string markdown)
    {
        var source = Path.GetFileName(path);
        var chunks = new List<KnowledgeChunk>();
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        string title = Path.GetFileNameWithoutExtension(path);
        var body = new List<string>();
        var index = 0;

        void Flush()
        {
            var text = string.Join("\n", body).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            chunks.Add(new KnowledgeChunk(
                Id: $"{source}#{index}",
                Source: source,
                Title: title,
                Text: text));
            index++;
            body.Clear();
        }

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();
                title = line[3..].Trim();
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                if (body.Count == 0)
                {
                    title = line[2..].Trim();
                    continue;
                }

                Flush();
                title = line[2..].Trim();
                continue;
            }

            body.Add(line);
        }

        Flush();
        return chunks;
    }
}
