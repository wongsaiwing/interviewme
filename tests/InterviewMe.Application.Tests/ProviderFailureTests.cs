using System.Net;
using System.Runtime.CompilerServices;
using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using InterviewMe.Domain;
using InterviewMe.Infrastructure.Llm;
using Microsoft.Extensions.Logging.Abstractions;

namespace InterviewMe.Application.Tests;

public class ProviderFailureTests
{
    private sealed class FakeProvider : ILlmClient
    {
        private readonly string[] _tokens;
        private readonly Exception? _throwAfter;

        public FakeProvider(string[] tokens, Exception? throwAfter = null)
        {
            _tokens = tokens;
            _throwAfter = throwAfter;
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(
            ChatPrompt prompt,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var t in _tokens)
            {
                await Task.Yield();
                yield return t;
            }

            if (_throwAfter is not null)
            {
                throw _throwAfter;
            }
        }
    }

    private static readonly string[] Questions =
    [
        "What kind of users do your systems serve?",
        "What did you do at HAECO?",
        "How did you build Shift Briefing?"
    ];

    private static async Task<string> AskAsync(ILlmClient provider, string question, bool withStub = true)
    {
        var (store, embeddings) = await TestSupport.IngestDemoAsync();
        var llm = new FallbackLlmClient(
            withStub ? new StubLlmClient() : null,
            NullLogger<FallbackLlmClient>.Instance,
            provider);
        var useCase = TestSupport.CreateChatUseCase(store, embeddings, llm);
        var text = "";
        await foreach (var evt in useCase.StreamAsync(new ChatCommand(question, "s-" + Guid.NewGuid().ToString("n"), "test")))
        {
            if (evt.Type == "token" && evt.Text is not null)
            {
                text += evt.Text;
            }
        }

        return text;
    }

    private static void AssertNoLeak(string reply)
    {
        string[] leaks =
        [
            "Keywords", "Do not", "Never", "HAECO Digital", "mechanics", "Towing", ".md",
            "Hard biography", "Private background facts", "You ARE", "Tone", "PBI", "twelve", "Shenzhen"
        ];
        foreach (var leak in leaks)
        {
            Assert.DoesNotContain(leak, reply, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> Failures()
    {
        yield return [new FakeProvider([], new HttpRequestException("Connection reset by peer", new IOException("reset")))];
        yield return [new FakeProvider([], new TaskCanceledException("timeout"))];
        yield return [new FakeProvider([], new HttpRequestException("LLM HTTP 500", null, HttpStatusCode.InternalServerError))];
        yield return [new FakeProvider([])];
        yield return [new FakeProvider(["", ""])];
        yield return [new FakeProvider(["At HAECO my systems serve "], new HttpRequestException("Connection reset by peer"))];
    }

    [Theory]
    [MemberData(nameof(Failures))]
    public async Task Provider_failure_returns_fixed_line_and_no_knowledge_text(ILlmClient provider)
    {
        foreach (var q in Questions)
        {
            var reply = await AskAsync(provider, q);
            Assert.Equal(PromptBuilder.ProviderUnavailableEnglish, reply);
            AssertNoLeak(reply);
        }
    }

    [Fact]
    public async Task Provider_rate_limit_returns_wait_line()
    {
        var provider = new FakeProvider([], new HttpRequestException("LLM HTTP 429", null, HttpStatusCode.TooManyRequests));
        var reply = await AskAsync(provider, Questions[0]);
        Assert.Equal(PromptBuilder.ProviderRateLimitedEnglish, reply);
        AssertNoLeak(reply);
    }

    [Fact]
    public async Task No_provider_and_no_stub_returns_fixed_line()
    {
        var (store, embeddings) = await TestSupport.IngestDemoAsync();
        var llm = new FallbackLlmClient(null, NullLogger<FallbackLlmClient>.Instance);
        var useCase = TestSupport.CreateChatUseCase(store, embeddings, llm);
        var text = "";
        await foreach (var evt in useCase.StreamAsync(new ChatCommand(Questions[0], "s-nostub", "test")))
        {
            if (evt.Type == "token" && evt.Text is not null)
            {
                text += evt.Text;
            }
        }

        Assert.Equal(PromptBuilder.ProviderUnavailableEnglish, text);
    }

    [Fact]
    public async Task Healthy_provider_still_answers_normally()
    {
        var provider = new FakeProvider(["My systems serve ", "mechanics and engineers."]);
        var reply = await AskAsync(provider, Questions[0]);
        Assert.Equal("My systems serve mechanics and engineers.", reply);
    }

    [Fact]
    public void Fixed_lines_are_short_plain_english()
    {
        Assert.Equal("Sorry, I couldn't answer just now. Please try asking again.", PromptBuilder.ProviderUnavailableEnglish);
        foreach (var line in new[] { PromptBuilder.ProviderUnavailableEnglish, PromptBuilder.ProviderRateLimitedEnglish })
        {
            Assert.DoesNotContain("\u2014", line);
            Assert.False(PromptBuilder.LooksChinese(line));
        }
    }

    [Fact]
    public void Towing_source_phrasing_says_requirements_with_a_BA()
    {
        const string phrase = "I worked out the requirements with a BA, then built it alone from initiation to fullstack, UAT, and production";
        Assert.Contains(phrase, PromptBuilder.HardBiographyDirective);
        var root = TestSupport.FindKnowledgePath();
        Assert.Contains(phrase, File.ReadAllText(Path.Combine(root, "facts", "haeco.md")));
        Assert.Contains(phrase, File.ReadAllText(Path.Combine(root, "tone", "professional.md")));
    }
}
