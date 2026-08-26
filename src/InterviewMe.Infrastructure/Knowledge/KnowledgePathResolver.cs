using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace InterviewMe.Infrastructure.Knowledge;

public static class KnowledgePathResolver
{
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["Knowledge:Path"];
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            candidates.Add(configured);
        }

        candidates.Add(Path.Combine(AppContext.BaseDirectory, "knowledge"));
        candidates.Add(Path.Combine(environment.ContentRootPath, "knowledge"));
        candidates.Add(Path.Combine(environment.ContentRootPath, "..", "..", "knowledge"));
        candidates.Add(Path.Combine(environment.ContentRootPath, "..", "..", "..", "..", "knowledge"));

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            candidates.Add(Path.Combine(dir.FullName, "knowledge"));
            dir = dir.Parent;
        }

        foreach (var candidate in candidates.Select(Path.GetFullPath).Distinct())
        {
            if (LooksLikeKnowledgeRoot(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not find the knowledge folder (expected knowledge/facts/*.md). Set Knowledge:Path.");
    }

    public static bool LooksLikeKnowledgeRoot(string candidate)
    {
        if (!Directory.Exists(candidate))
        {
            return false;
        }

        var facts = Path.Combine(candidate, "facts");
        if (Directory.Exists(facts) &&
            Directory.EnumerateFiles(facts, "*.md", SearchOption.AllDirectories).Any())
        {
            return true;
        }

        return Directory.EnumerateFiles(candidate, "*.md", SearchOption.TopDirectoryOnly)
            .Any(f => !string.Equals(Path.GetFileName(f), "README.md", StringComparison.OrdinalIgnoreCase));
    }
}
