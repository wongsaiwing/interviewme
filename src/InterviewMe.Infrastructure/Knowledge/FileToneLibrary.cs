using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;

namespace InterviewMe.Infrastructure.Knowledge;

/// <summary>
/// Loads markdown from knowledge/tone. These files are prompt-only and are never embedded.
/// </summary>
public sealed class FileToneLibrary : IToneLibrary
{
    private readonly string _text;

    public FileToneLibrary(string knowledgePath)
    {
        var toneDir = Path.Combine(knowledgePath, "tone");
        if (!Directory.Exists(toneDir))
        {
            _text = PromptBuilder.DefaultTone;
            return;
        }

        var files = Directory.GetFiles(toneDir, "*.md", SearchOption.TopDirectoryOnly)
            .Where(f => !string.Equals(Path.GetFileName(f), "README.md", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
        {
            _text = PromptBuilder.DefaultTone;
            return;
        }

        _text = string.Join("\n\n", files.Select(File.ReadAllText).Where(t => !string.IsNullOrWhiteSpace(t)));
        if (string.IsNullOrWhiteSpace(_text))
        {
            _text = PromptBuilder.DefaultTone;
        }
    }

    public string GetFewShots() => _text;
}
