namespace InterviewMe.Application.Abstractions;

/// <summary>
/// Tone / few-shot text for the system prompt. Must never be written into the vector store.
/// </summary>
public interface IToneLibrary
{
    string GetFewShots();
}
