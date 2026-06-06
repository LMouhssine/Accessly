using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Accessly.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Accessly.Infrastructure.Ai;

/// <summary>
/// Optional assistant backed by an OpenAI-compatible chat completions API. Disabled by default;
/// only used when <c>Ai:Provider=OpenAiCompatible</c> with a base URL and API key configured.
/// </summary>
public sealed class OpenAiCompatibleProvider(HttpClient httpClient, IOptions<AiOptions> options) : IAiProvider
{
    private readonly AiOptions _options = options.Value;

    public string Name => "OpenAiCompatible";

    public Task<string> GenerateEventDescriptionAsync(AiEventContext context, CancellationToken cancellationToken = default)
        => ChatAsync(
            "You write concise, engaging event descriptions (3-4 sentences).",
            $"Event: '{context.Title}'. Category: {context.Category}. Audience: {context.Audience}. Topics: {context.Topics}.",
            cancellationToken);

    public async Task<IReadOnlyList<string>> SuggestTagsAsync(AiEventContext context, CancellationToken cancellationToken = default)
    {
        var text = await ChatAsync(
            "You suggest short, lowercase, comma-separated tags.",
            $"Suggest up to 8 tags for the event '{context.Title}' ({context.Category}). Topics: {context.Topics}.",
            cancellationToken);

        return text
            .Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.Trim('#', ' ').ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Take(8)
            .ToList();
    }

    public Task<string> GenerateReminderEmailAsync(AiEventContext context, CancellationToken cancellationToken = default)
        => ChatAsync(
            "You write friendly, concise reminder emails.",
            $"Write a reminder email for '{context.Title}' starting {context.StartAt:f} at {context.VenueName}.",
            cancellationToken);

    public Task<string> GenerateAgendaAsync(AiEventContext context, CancellationToken cancellationToken = default)
        => ChatAsync(
            "You design clear event agendas as a bulleted timeline.",
            $"Propose an agenda for '{context.Title}' lasting {context.DurationHours ?? 3} hours. Topics: {context.Topics}.",
            cancellationToken);

    public Task<string> GenerateAnnouncementAsync(AiEventContext context, CancellationToken cancellationToken = default)
        => ChatAsync(
            "You write short promotional announcements.",
            $"Write a short announcement for '{context.Title}' ({context.Category}) at {context.VenueName}.",
            cancellationToken);

    public Task<string> SummarizeFeedbackAsync(string eventTitle, IReadOnlyList<FeedbackSnippet> feedback, CancellationToken cancellationToken = default)
    {
        var joined = string.Join("\n", feedback.Select(f => $"[{f.Rating}/5] {f.Comment}"));
        return ChatAsync(
            "You objectively summarize attendee feedback.",
            $"Summarize the following feedback for '{eventTitle}':\n{joined}",
            cancellationToken);
    }

    private async Task<string> ChatAsync(string system, string user, CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = _options.Model ?? "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user },
            },
            temperature = 0.7,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}
