using InterviewMe.Infrastructure.Knowledge;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InterviewMe.Api.Tests;

public sealed class InterviewMeApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Llm:ApiKey", "");
        builder.UseSetting("Chat:RateLimitPerMinute", "1000");
        builder.UseSetting("Knowledge:Path", FindKnowledgePath());
    }

    private static string FindKnowledgePath()
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
}
