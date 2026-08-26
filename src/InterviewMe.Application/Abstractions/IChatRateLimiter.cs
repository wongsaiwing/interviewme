namespace InterviewMe.Application.Abstractions;

public interface IChatRateLimiter
{
    bool TryAcquire(string clientKey);
}
