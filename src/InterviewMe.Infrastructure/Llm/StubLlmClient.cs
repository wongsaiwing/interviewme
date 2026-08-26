using System.Runtime.CompilerServices;
using System.Text;
using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using InterviewMe.Domain;

namespace InterviewMe.Infrastructure.Llm;

/// <summary>
/// Grounded stub used when no OpenAI-compatible API key is configured.
/// Streams a reply built only from retrieved chunks so docker compose works with zero accounts.
/// </summary>
public sealed class StubLlmClient : ILlmClient
{
    public async IAsyncEnumerable<string> StreamCompletionAsync(
        ChatPrompt prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var text = Compose(prompt);
        foreach (var piece in Split(text, 14))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return piece;
            await Task.Delay(8, cancellationToken);
        }
    }

    public static string Compose(ChatPrompt prompt)
    {
        var user = prompt.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";

        if (PromptBuilder.IsIntroduction(user))
        {
            return ComposeIntroduction(prompt);
        }

        if (PromptBuilder.IsIcebreaker(user))
        {
            return PromptBuilder.IcebreakerReply(user);
        }

        if (PromptBuilder.IsOffTopic(user))
        {
            return PromptBuilder.OffTopicRefuse(user);
        }

        if (!prompt.HasGrounding || prompt.Facts.Count == 0)
        {
            return PromptBuilder.MissingDetail(user);
        }

        return ComposeFromFacts(prompt.Facts, spoken: true);
    }

    private static string ComposeIntroduction(ChatPrompt prompt)
    {
        if (prompt.HasGrounding && prompt.Facts.Count > 0)
        {
            var body = ComposeFromFacts(prompt.Facts, spoken: false);
            var hasSilas = ContainsIgnore(body, "Silas");
            var hasHaeco = ContainsIgnore(body, "HAECO");
            var hasStack = ContainsIgnore(body, "full-stack") || ContainsIgnore(body, "full stack");
            if (hasSilas && hasHaeco && hasStack)
            {
                return body;
            }
        }

        return "I'm Silas Wong, a full-stack developer based in Hong Kong. " +
               "In my current role at HAECO I'm an Assistant Solution Analyst, mostly .NET and React. " +
               "I'd say the work sits end-to-end, from requirements through to production.";
    }

    private static string ComposeFromFacts(IReadOnlyList<RetrievedFact> facts, bool spoken)
    {
        var sb = new StringBuilder();
        if (spoken)
        {
            sb.Append("In my current role I'd put it this way. ");
        }

        var used = 0;
        foreach (var fact in facts)
        {
            var snippet = Collapse(fact.Text);
            if (snippet.Length == 0)
            {
                continue;
            }

            if (used > 0)
            {
                sb.Append(' ');
            }

            sb.Append(snippet);
            if (!snippet.EndsWith('.'))
            {
                sb.Append('.');
            }

            used++;
            if (used >= 3)
            {
                break;
            }
        }

        return sb.ToString();
    }

    private static bool ContainsIgnore(string text, string value) =>
        text.Contains(value, StringComparison.OrdinalIgnoreCase);

    private static string Collapse(string text)
    {
        var kept = text.Split('\n')
            .Where(line =>
            {
                var trim = line.TrimStart();
                return !trim.StartsWith("Keywords:", StringComparison.OrdinalIgnoreCase)
                       && !trim.StartsWith("Email:", StringComparison.OrdinalIgnoreCase)
                       && !trim.StartsWith("Phone:", StringComparison.OrdinalIgnoreCase);
            });
        var flat = string.Join(' ', kept).Replace('\n', ' ').Trim();
        while (flat.Contains("  ", StringComparison.Ordinal))
        {
            flat = flat.Replace("  ", " ", StringComparison.Ordinal);
        }

        if (flat.Length > 280)
        {
            flat = flat[..277].TrimEnd() + "...";
        }

        return flat;
    }

    private static IEnumerable<string> Split(string text, int size)
    {
        for (var i = 0; i < text.Length; i += size)
        {
            yield return text.Substring(i, Math.Min(size, text.Length - i));
        }
    }
}
