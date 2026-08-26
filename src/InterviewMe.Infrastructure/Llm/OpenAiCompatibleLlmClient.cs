using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using InterviewMe.Application.Abstractions;
using InterviewMe.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterviewMe.Infrastructure.Llm;

public sealed class OpenAiCompatibleLlmClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly LlmOptions _options;
    private readonly ILogger<OpenAiCompatibleLlmClient> _logger;

    public OpenAiCompatibleLlmClient(
        HttpClient http,
        IOptions<LlmOptions> options,
        ILogger<OpenAiCompatibleLlmClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        ChatPrompt prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Combine(_options.BaseUrl, "chat/completions"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(BuildBody(prompt), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var code = (int)response.StatusCode;
            _logger.LogWarning("LLM HTTP {StatusCode}", code);
            await response.Content.CopyToAsync(Stream.Null, cancellationToken);
            throw new HttpRequestException($"LLM HTTP {code}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var payload = line["data:".Length..].Trim();
            if (payload == "[DONE]")
            {
                yield break;
            }

            var token = ReadDelta(payload);
            if (!string.IsNullOrEmpty(token))
            {
                yield return token;
            }
        }
    }

    internal string BuildBody(ChatPrompt prompt)
    {
        var messages = prompt.Messages.Select(m => new { role = m.Role, content = m.Content });
        var payload = new
        {
            model = _options.Model,
            stream = true,
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokens,
            thinking = new { type = "disabled" },
            messages
        };
        return JsonSerializer.Serialize(payload);
    }

    private static string Combine(string baseUrl, string relative)
    {
        return $"{baseUrl.TrimEnd('/')}/{relative.TrimStart('/')}";
    }

    private string? ReadDelta(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return null;
            }

            var choice = choices[0];
            if (!choice.TryGetProperty("delta", out var delta))
            {
                return null;
            }

            if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
            {
                return content.GetString();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "Skipping non-JSON SSE chunk");
        }

        return null;
    }
}
