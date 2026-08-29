using System.Text.Json;
using System.Text.Json.Serialization;
using InterviewMe.Application;
using InterviewMe.Application.Abstractions;
using InterviewMe.Application.Chat;
using InterviewMe.Application.Profile;
using InterviewMe.Infrastructure;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInterviewMeApplication(builder.Configuration);
builder.Services.AddInterviewMeInfrastructure(builder.Configuration, builder.Environment);

var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                     ?? ["http://localhost:5173", "http://127.0.0.1:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("spa", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithHeaders("Content-Type")
            .WithMethods("GET", "POST");
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ingestor = scope.ServiceProvider.GetRequiredService<IKnowledgeIngestor>();
    await ingestor.IngestAsync();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self'; connect-src 'self'";
    await next();
});
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
    var sessionId = SanitizeSessionId(body.SessionId);
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

app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallback("/knowledge/{**path}", () => Results.NotFound());
app.MapFallback("/swagger/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");
app.Run();


static string SanitizeSessionId(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw))
    {
        return Guid.NewGuid().ToString("n");
    }

    var trimmed = raw.Trim();
    var buf = new char[Math.Min(trimmed.Length, 64)];
    var n = 0;
    foreach (var c in trimmed)
    {
        if (n >= 64)
        {
            break;
        }

        if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
        {
            buf[n++] = c;
        }
    }

    return n == 0 ? Guid.NewGuid().ToString("n") : new string(buf, 0, n);
}

public sealed record ChatRequestBody(string? Message, string? SessionId);

public partial class Program;
