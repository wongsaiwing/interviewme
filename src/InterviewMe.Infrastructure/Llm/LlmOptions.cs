namespace InterviewMe.Infrastructure.Llm;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.deepseek.com";
    public string Model { get; set; } = "deepseek-v4-flash";
    public int MaxTokens { get; set; } = 180;
    public float Temperature { get; set; } = 0.85f;

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
