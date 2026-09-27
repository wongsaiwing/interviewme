namespace InterviewMe.Application.Abstractions;

/// <summary>
/// Thrown when the LLM provider fails (connection reset, timeout, HTTP error, rate limit, empty reply).
/// The chat use case turns this into a short fixed visitor line. Never fall back to raw knowledge text.
/// </summary>
public sealed class LlmUnavailableException : Exception
{
    public LlmUnavailableException(bool rateLimited, Exception? inner = null)
        : base(rateLimited ? "LLM provider rate limited" : "LLM provider unavailable", inner)
    {
        RateLimited = rateLimited;
    }

    public bool RateLimited { get; }
}
