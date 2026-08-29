using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace InterviewMe.Api.Tests;

public class ChatApiTests : IClassFixture<InterviewMeApiFactory>
{
    private readonly InterviewMeApiFactory _factory;

    public ChatApiTests(InterviewMeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Profile_returns_silas_wong()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/profile");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Silas Wong", json);
        Assert.Contains("HAECO", json);
        Assert.DoesNotContain("Avery Chen", json);
    }

    [Fact]
    public async Task Chat_stream_answers_haeco_from_resume()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/stream")
        {
            Content = JsonContent.Create(new { message = "What did you do at HAECO?", sessionId = "api-test-1" })
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        var text = ReadTokens(body);
        Assert.Contains("HAECO", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Avery", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data:", body);
        Assert.DoesNotContain("sources", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".md", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HardBiography", body, StringComparison.Ordinal);
        Assert.DoesNotContain("current-package", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_api_paths_are_not_found()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/facts");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Silas Wong", json);
        Assert.DoesNotContain("compensation.md", json);
    }

    [Fact]
    public async Task Knowledge_files_are_not_public()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/knowledge/facts/current-package.md");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("24000", body);
        Assert.DoesNotContain("14.5", body);
    }

    private static string ReadTokens(string sse)
    {
        var sb = new StringBuilder();
        foreach (var line in sse.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var payload = trimmed["data:".Length..].Trim();
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("type", out var type) &&
                type.GetString() == "token" &&
                doc.RootElement.TryGetProperty("text", out var text))
            {
                sb.Append(text.GetString());
            }
        }

        return sb.ToString();
    }
}
