using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using InterviewMe.Application.Profile;
using InterviewMe.Infrastructure.Conversation;
using InterviewMe.Infrastructure.Embeddings;
using InterviewMe.Infrastructure.Knowledge;
using InterviewMe.Infrastructure.Llm;
using InterviewMe.Infrastructure.RateLimiting;
using InterviewMe.Infrastructure.VectorStore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InterviewMe.Application.Tests;

internal static class TestSupport
{
    internal static string FindKnowledgePath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "knowledge");
            if (KnowledgePathResolver.LooksLikeKnowledgeRoot(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("knowledge markdown folder");
    }

    internal static ChatUseCase CreateChatUseCase(
        IVectorStore store,
        IEmbeddingClient embeddings,
        ILlmClient llm,
        ChatOptions? chatOptions = null)
    {
        var opts = chatOptions ?? new ChatOptions { MinScore = 0.08f, TopK = 4, RateLimitPerMinute = 1000 };
        return new ChatUseCase(
            embeddings,
            store,
            llm,
            new InMemoryRateLimiter(Options.Create(opts)),
            new InMemoryConversationStore(),
            new DefaultToneLibrary(),
            new PromptBuilder(),
            Options.Create(opts),
            Options.Create(new ProfileOptions()));
    }

    internal static async Task<(LangChainVectorStore Store, HashingEmbeddingModel Embeddings)> IngestDemoAsync()
    {
        var store = new LangChainVectorStore();
        var embeddings = new HashingEmbeddingModel();
        var ingestor = new MarkdownKnowledgeIngestor(
            embeddings,
            store,
            NullLogger<MarkdownKnowledgeIngestor>.Instance,
            FindKnowledgePath());
        await ingestor.IngestAsync();
        return (store, embeddings);
    }
}
