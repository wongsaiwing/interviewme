using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Profile;
using InterviewMe.Domain;
using Microsoft.Extensions.Options;

namespace InterviewMe.Application.Chat;

public sealed class ChatUseCase
{
    internal const string IntroExpandedQuery =
        "Silas Wong full-stack HAECO profile summary introduce yourself";

    private readonly IEmbeddingClient _embeddings;
    private readonly IVectorStore _store;
    private readonly ILlmClient _llm;
    private readonly IChatRateLimiter _rateLimiter;
    private readonly IConversationStore _conversations;
    private readonly IToneLibrary _tone;
    private readonly PromptBuilder _promptBuilder;
    private readonly ChatOptions _chatOptions;
    private readonly ProfileOptions _profile;

    public ChatUseCase(
        IEmbeddingClient embeddings,
        IVectorStore store,
        ILlmClient llm,
        IChatRateLimiter rateLimiter,
        IConversationStore conversations,
        IToneLibrary tone,
        PromptBuilder promptBuilder,
        IOptions<ChatOptions> chatOptions,
        IOptions<ProfileOptions> profile)
    {
        _embeddings = embeddings;
        _store = store;
        _llm = llm;
        _rateLimiter = rateLimiter;
        _conversations = conversations;
        _tone = tone;
        _promptBuilder = promptBuilder;
        _chatOptions = chatOptions.Value;
        _profile = profile.Value;
    }

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatCommand command,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var error = Validate(command);
        if (error is not null)
        {
            yield return ChatStreamEvent.Failure(error);
            yield break;
        }

        if (!_rateLimiter.TryAcquire(command.ClientKey))
        {
            yield return ChatStreamEvent.Failure("Too many questions just now. Wait a moment and try again.");
            yield break;
        }

        var history = _conversations.GetRecent(command.SessionId, _chatOptions.ConversationTurns);
        var facts = await RetrieveFactsAsync(command.Message, cancellationToken);

        var prompt = _promptBuilder.Build(_profile.Name, command.Message, history, facts, _tone.GetFewShots());

        var citations = facts
            .Select(f => new SourceCitation(f.Id, f.Source, f.Title, f.Score))
            .ToList();
        yield return ChatStreamEvent.WithSources(citations);

        var assembled = new System.Text.StringBuilder();
        await foreach (var token in _llm.StreamCompletionAsync(prompt, cancellationToken))
        {
            assembled.Append(token);
        }

        var reply = BiographyGuard.Sanitize(assembled.ToString());
        foreach (var piece in ChunkReply(reply, 24))
        {
            yield return ChatStreamEvent.Token(piece);
        }

        _conversations.Append(command.SessionId, new ChatMessage("user", command.Message.Trim()));
        _conversations.Append(command.SessionId, new ChatMessage("assistant", reply));

        yield return ChatStreamEvent.Done();
    }

    internal async Task<List<RetrievedFact>> RetrieveFactsAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddings.EmbedAsync(message, cancellationToken);
        var retrieved = await _store.SearchAsync(
            embedding,
            message,
            _chatOptions.TopK,
            cancellationToken);

        var facts = retrieved
            .Where(f => f.Score >= _chatOptions.MinScore)
            .ToList();

        facts = await ExpandSameSourceAsync(facts, cancellationToken);

        var expandIntro = PromptBuilder.IsIntroduction(message)
                          || (facts.Count == 0 && PromptBuilder.LooksLikeAboutMe(message));
        if (!expandIntro)
        {
            return facts;
        }

        var expandedEmbedding = await _embeddings.EmbedAsync(IntroExpandedQuery, cancellationToken);
        var expanded = await _store.SearchAsync(
            expandedEmbedding,
            IntroExpandedQuery,
            Math.Max(_chatOptions.TopK, 6),
            cancellationToken);

        IReadOnlyList<RetrievedFact> profileChunks;
        try
        {
            profileChunks = await _store.GetBySourcePrefixAsync("profile.md", cancellationToken);
        }
        catch (NotImplementedException)
        {
            profileChunks = [];
        }

        return MergeIntroFacts(facts, retrieved, expanded, profileChunks);
    }

    private List<RetrievedFact> MergeIntroFacts(
        List<RetrievedFact> scored,
        IReadOnlyList<RetrievedFact> firstSearch,
        IReadOnlyList<RetrievedFact> expanded,
        IReadOnlyList<RetrievedFact> profileChunks)
    {
        var byId = new Dictionary<string, RetrievedFact>(StringComparer.Ordinal);

        void Consider(RetrievedFact fact, bool bypassMinScore)
        {
            if (byId.ContainsKey(fact.Id))
            {
                return;
            }

            var isProfile = fact.Source.StartsWith("profile.md", StringComparison.OrdinalIgnoreCase);
            // MinScore must not drop intro profile chunks.
            if (bypassMinScore || isProfile || fact.Score >= _chatOptions.MinScore)
            {
                byId[fact.Id] = fact;
            }
        }

        foreach (var fact in scored)
        {
            byId[fact.Id] = fact;
        }

        foreach (var fact in profileChunks)
        {
            Consider(fact, bypassMinScore: true);
        }

        foreach (var fact in expanded)
        {
            Consider(fact, bypassMinScore: false);
        }

        foreach (var fact in firstSearch)
        {
            Consider(fact, bypassMinScore: false);
        }

        return byId.Values
            .OrderByDescending(f => f.Score)
            .Take(Math.Max(_chatOptions.TopK, 6))
            .ToList();
    }


    private static readonly HashSet<string> SameSourceExpand =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "internship-compathnion.md",
            "swc.md",
            "haeco.md"
        };

    private async Task<List<RetrievedFact>> ExpandSameSourceAsync(
        List<RetrievedFact> facts,
        CancellationToken cancellationToken)
    {
        if (facts.Count == 0)
        {
            return facts;
        }

        var byId = facts.ToDictionary(f => f.Id, StringComparer.Ordinal);
        var sources = facts
            .Select(f => f.Source)
            .Where(s => SameSourceExpand.Contains(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var source in sources)
        {
            IReadOnlyList<RetrievedFact> siblings;
            try
            {
                siblings = await _store.GetBySourcePrefixAsync(source, cancellationToken);
            }
            catch (NotImplementedException)
            {
                continue;
            }

            var best = facts
                .Where(f => f.Source.Equals(source, StringComparison.OrdinalIgnoreCase))
                .Select(f => f.Score)
                .DefaultIfEmpty(0.5f)
                .Max();

            foreach (var sibling in siblings)
            {
                if (byId.ContainsKey(sibling.Id))
                {
                    continue;
                }

                byId[sibling.Id] = new RetrievedFact(
                    sibling.Id,
                    sibling.Source,
                    sibling.Title,
                    sibling.Text,
                    Math.Min(best * 0.98f, 0.99f));
            }
        }

        return byId.Values
            .OrderByDescending(f => f.Score)
            .Take(Math.Max(_chatOptions.TopK, 8))
            .ToList();
    }

    private static IEnumerable<string> ChunkReply(string text, int size)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        for (var i = 0; i < text.Length; i += size)
        {
            yield return text.Substring(i, Math.Min(size, text.Length - i));
        }
    }

    public string? Validate(ChatCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Message))
        {
            return "Message is required.";
        }

        if (command.Message.Trim().Length > _chatOptions.MaxMessageLength)
        {
            return $"Message is too long (max {_chatOptions.MaxMessageLength} characters).";
        }

        if (string.IsNullOrWhiteSpace(command.SessionId))
        {
            return "Session id is required.";
        }

        return null;
    }
}
