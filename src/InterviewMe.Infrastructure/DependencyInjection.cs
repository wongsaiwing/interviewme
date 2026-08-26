using InterviewMe.Application.Abstractions;
using InterviewMe.Infrastructure.Conversation;
using InterviewMe.Infrastructure.Embeddings;
using InterviewMe.Infrastructure.Knowledge;
using InterviewMe.Infrastructure.Llm;
using InterviewMe.Infrastructure.RateLimiting;
using InterviewMe.Infrastructure.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InterviewMe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInterviewMeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<LlmOptions>(opts =>
        {
            configuration.GetSection(LlmOptions.SectionName).Bind(opts);
            var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                opts.ApiKey = envKey;
            }
        });

        services.AddSingleton<HashingEmbeddingClient>();
        services.AddSingleton<HashingEmbeddingModel>();
        services.AddSingleton<IEmbeddingClient>(sp => sp.GetRequiredService<HashingEmbeddingModel>());
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            sp.GetRequiredService<HashingEmbeddingModel>());
        services.AddSingleton<IVectorStore, LangChainVectorStore>();
        services.AddSingleton<IChatRateLimiter, InMemoryRateLimiter>();
        services.AddSingleton<IConversationStore, InMemoryConversationStore>();
        services.AddSingleton<StubLlmClient>();

        var llm = new LlmOptions();
        configuration.GetSection(LlmOptions.SectionName).Bind(llm);
        if (string.IsNullOrWhiteSpace(llm.ApiKey))
        {
            llm.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if (llm.HasApiKey)
        {
            services.AddHttpClient<OpenAiCompatibleLlmClient>();
            services.AddSingleton<ILlmClient>(sp =>
                new FallbackLlmClient(
                    sp.GetRequiredService<StubLlmClient>(),
                    sp.GetRequiredService<ILogger<FallbackLlmClient>>(),
                    sp.GetRequiredService<OpenAiCompatibleLlmClient>()));
        }
        else
        {
            services.AddSingleton<ILlmClient>(sp =>
                new FallbackLlmClient(
                    sp.GetRequiredService<StubLlmClient>(),
                    sp.GetRequiredService<ILogger<FallbackLlmClient>>()));
        }

        var knowledgePath = KnowledgePathResolver.Resolve(configuration, environment);
        services.AddSingleton<IToneLibrary>(_ => new FileToneLibrary(knowledgePath));
        services.AddSingleton<IKnowledgeIngestor>(sp =>
            new MarkdownKnowledgeIngestor(
                sp.GetRequiredService<IEmbeddingClient>(),
                sp.GetRequiredService<IVectorStore>(),
                sp.GetRequiredService<ILogger<MarkdownKnowledgeIngestor>>(),
                knowledgePath));

        return services;
    }
}
