using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using InterviewMe.Application.Profile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewMe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInterviewMeApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ChatOptions>(configuration.GetSection(ChatOptions.SectionName));
        services.Configure<ProfileOptions>(configuration.GetSection(ProfileOptions.SectionName));
        services.AddSingleton<PromptBuilder>();
        services.AddSingleton<IToneLibrary, DefaultToneLibrary>();
        services.AddSingleton<ChatUseCase>();
        return services;
    }
}
