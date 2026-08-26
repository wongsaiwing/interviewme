using System.Text.RegularExpressions;
using InterviewMe.Application.Abstractions;

namespace InterviewMe.Infrastructure.Embeddings;

/// <summary>
/// Deterministic local embeddings so the site runs with zero cloud accounts.
/// Not a substitute for a real embedding model; good enough for a small markdown corpus.
/// </summary>
public sealed class HashingEmbeddingClient : IEmbeddingClient
{
    public const int Dimensions = 256;
    private static readonly Regex LatinPattern = new(@"[a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex CjkRun = new(@"[\u3400-\u9fff]+", RegexOptions.Compiled);
    private static readonly HashSet<string> Stop =
    [
        "what", "is", "your", "the", "and", "to", "of", "for", "in", "on", "this", "that",
        "are", "you", "me", "my", "with", "from", "was", "were", "been", "have", "has",
        "not", "do", "does", "did", "can", "about", "tell", "please", "who", "how",
        "when", "where", "why", "which", "or", "an", "as", "at", "be", "by", "it",
        "its", "we", "they", "their", "our", "id", "name"
    ];

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var vector = new float[Dimensions];
        var tokens = Tokenize(text);
        for (var i = 0; i < tokens.Count; i++)
        {
            Add(vector, tokens[i], 1f);
            if (i + 1 < tokens.Count)
            {
                Add(vector, tokens[i] + "_" + tokens[i + 1], 0.7f);
            }
        }

        Normalize(vector);
        return Task.FromResult(vector);
    }

    public static IReadOnlyList<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        foreach (Match match in LatinPattern.Matches(text.ToLowerInvariant()))
        {
            if (match.Value.Length < 2 || Stop.Contains(match.Value))
            {
                continue;
            }

            tokens.Add(match.Value);
        }

        foreach (Match match in CjkRun.Matches(text))
        {
            var run = match.Value;
            if (run.Length >= 2)
            {
                tokens.Add(run);
            }

            for (var i = 0; i < run.Length - 1; i++)
            {
                tokens.Add(run.Substring(i, 2));
            }
        }

        return tokens;
    }

    private static void Add(float[] vector, string token, float weight)
    {
        var bucket = (int)((uint)StableHash(token) % Dimensions);
        vector[bucket] += weight;
    }

    internal static int StableHash(string value)
    {
        unchecked
        {
            var hash = 23;
            foreach (var c in value)
            {
                hash = (hash * 31) + c;
            }

            return hash;
        }
    }

    internal static void Normalize(float[] vector)
    {
        double sum = 0;
        foreach (var v in vector)
        {
            sum += v * (double)v;
        }

        var norm = Math.Sqrt(sum);
        if (norm < 1e-8)
        {
            return;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(vector[i] / norm);
        }
    }
}
