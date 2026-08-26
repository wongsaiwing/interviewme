using System.Text.Json;
using System.Text.Json.Serialization;
using InterviewMe.Application;
using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using InterviewMe.Application.Profile;
using InterviewMe.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// IP now, domain later: do not reject 47.82.69.6 or swwdomain.hk hostnames.
// HTTPS is not required yet; HTTP on :80 is the live HR URL.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                               | ForwardedHeaders.XForwardedProto
                               | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddInterviewMeApplication(builder.Configuration);
builder.Services.AddInterviewMeInfrastructure(builder.Configuration, builder.Environment);

var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                     ?? ["http://localhost:5173", "http://127.0.0.1:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("spa", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ingestor = scope.ServiceProvider.GetRequiredService<IKnowledgeIngestor>();
    await ingestor.IngestAsync();
}

app.UseForwardedHeaders();
app.UseCors("spa");
app.UseDefaultFiles();
app.UseStaticFiles();

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

app.MapGet("/api/health", () => Results.Json(new { status = "ok" }));

app.MapGet("/api/profile", (IOptions<ProfileOptions> options) =>
{
    var p = options.Value;
    return Results.Json(new ProfileDto(
        p.Name,
        p.Title,
        p.ShortBio,
        p.AvatarInitials,
        p.SuggestedQuestions));
});

app.MapPost("/api/chat/stream", async (
    ChatRequestBody body,
    HttpContext http,
    ChatUseCase chat,
    CancellationToken cancellationToken) =>
{
    var sessionId = string.IsNullOrWhiteSpace(body.SessionId)
        ? Guid.NewGuid().ToString("n")
        : body.SessionId.Trim();
    var clientKey = http.Connection.RemoteIpAddress?.ToString() ?? "local";
    var command = new ChatCommand(body.Message ?? string.Empty, sessionId, clientKey);

    http.Response.Headers.CacheControl = "no-cache";
    http.Response.ContentType = "text/event-stream";

    await foreach (var evt in chat.StreamAsync(command, cancellationToken))
    {
        var json = JsonSerializer.Serialize(evt, jsonOptions);
        await http.Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await http.Response.Body.FlushAsync(cancellationToken);
        if (evt.Type is "done" or "error")
        {
            break;
        }
    }
});

app.MapFallbackToFile("index.html");
app.Run();

public sealed record ChatRequestBody(string? Message, string? SessionId);

public partial class Program;
