using Accessly.Application.Common;
using Accessly.Application.Common.Interfaces;

namespace Accessly.Application.Features.Ai;

public sealed record AiEventRequest(
    string Title,
    string? Category = null,
    string? Audience = null,
    string? Topics = null,
    DateTimeOffset? StartAt = null,
    string? VenueName = null,
    int? DurationHours = null);

public sealed record AiTextResponse(string Provider, string Text);

public sealed record AiTagsResponse(string Provider, IReadOnlyList<string> Tags);

/// <summary>Orchestrates the AI provider for event-related generation and records audit entries.</summary>
public sealed class AiEventAssistantService(IAiProvider provider, IAuditLogger audit)
{
    public async Task<AiTextResponse> EventDescriptionAsync(AiEventRequest request, CancellationToken cancellationToken = default)
    {
        var text = await provider.GenerateEventDescriptionAsync(Map(request), cancellationToken);
        await LogAsync("event-description", request.Title, cancellationToken);
        return new AiTextResponse(provider.Name, text);
    }

    public async Task<AiTagsResponse> SuggestTagsAsync(AiEventRequest request, CancellationToken cancellationToken = default)
    {
        var tags = await provider.SuggestTagsAsync(Map(request), cancellationToken);
        await LogAsync("event-tags", request.Title, cancellationToken);
        return new AiTagsResponse(provider.Name, tags);
    }

    public async Task<AiTextResponse> ReminderEmailAsync(AiEventRequest request, CancellationToken cancellationToken = default)
    {
        var text = await provider.GenerateReminderEmailAsync(Map(request), cancellationToken);
        await LogAsync("reminder-email", request.Title, cancellationToken);
        return new AiTextResponse(provider.Name, text);
    }

    public async Task<AiTextResponse> AgendaAsync(AiEventRequest request, CancellationToken cancellationToken = default)
    {
        var text = await provider.GenerateAgendaAsync(Map(request), cancellationToken);
        await LogAsync("event-agenda", request.Title, cancellationToken);
        return new AiTextResponse(provider.Name, text);
    }

    public async Task<AiTextResponse> AnnouncementAsync(AiEventRequest request, CancellationToken cancellationToken = default)
    {
        var text = await provider.GenerateAnnouncementAsync(Map(request), cancellationToken);
        await LogAsync("announcement", request.Title, cancellationToken);
        return new AiTextResponse(provider.Name, text);
    }

    public async Task<AiTextResponse> SummarizeFeedbackAsync(string eventTitle, IReadOnlyList<FeedbackSnippet> feedback, CancellationToken cancellationToken = default)
    {
        var text = await provider.SummarizeFeedbackAsync(eventTitle, feedback, cancellationToken);
        await LogAsync("feedback-summary", eventTitle, cancellationToken);
        return new AiTextResponse(provider.Name, text);
    }

    private Task LogAsync(string capability, string subject, CancellationToken cancellationToken)
        => audit.LogAsync(AuditActions.AiGenerated, "Ai", null, new { capability, subject, provider = provider.Name }, cancellationToken);

    private static AiEventContext Map(AiEventRequest request) =>
        new(request.Title, request.Category, request.Audience, request.Topics, request.StartAt, request.VenueName, request.DurationHours);
}
